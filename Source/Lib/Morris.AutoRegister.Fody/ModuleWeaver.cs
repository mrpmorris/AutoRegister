using Fody;
using Microsoft.Extensions.DependencyInjection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Morris.AutoRegister.Fody.Extensions;
using Morris.AutoRegister.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Morris.AutoRegister.Fody;

public class ModuleWeaver : BaseModuleWeaver
{
	public const string ManifestHeader = "Module,Attribute,Scope,ServiceType,ServiceImplementation";

	public override IEnumerable<string> GetAssembliesForScanning()
	{
		yield return "netstandard";
		yield return "mscorlib";
	}

	public override void Execute()
	{
		ProcessClasses();
		RemoveDependency();
	}

	private void ProcessClasses()
	{
		var manifestBuilder = new StringBuilder();
		manifestBuilder.AppendLinuxLine(ManifestHeader);

		IEnumerable<TypeDefinition> classesToScan =
			ModuleDefinition
			.GetAllTypes()
			.Where(x => x.IsClass)
			.Where(x => !x.HasGenericParameters)
			.Where(x => !x.IsAbstract)
			.Where(x => !x.Name.StartsWith("<"))
			.OrderBy(x => x.FullName);

		foreach (TypeDefinition type in classesToScan)
			ScanType(type, manifestBuilder, classesToScan);

		WriteManifestFile(manifestBuilder.ToString());
	}

	private void ScanType(
		TypeDefinition type,
		StringBuilder manifestBuilder,
		IEnumerable<TypeDefinition> classesToScan)
	{
		var autoRegisterAttributes = new List<AutoRegisterAttributeData>();
		var autoRegisterFilterAttributes = new List<AutoRegisterFilterAttributeData>();
		for (int i = type.CustomAttributes.Count - 1; i >= 0; i--)
		{
			CustomAttribute currentAttribute = type.CustomAttributes[i];
			if (currentAttribute.AttributeType.FullName == "Morris.AutoRegister.AutoRegisterAttribute")
			{
				AutoRegisterAttributeData autoRegisterAttributeData = AutoRegisterAttributeDataFactory.Create(currentAttribute);
				autoRegisterAttributes.Add(autoRegisterAttributeData);
				type.CustomAttributes.RemoveAt(i);
			}
			else if (currentAttribute.AttributeType.FullName == "Morris.AutoRegister.AutoRegisterFilterAttribute")
			{
				AutoRegisterFilterAttributeData autoRegisterFilterAttributeData = AutoRegisterFilterAttributeDataFactory.Create(currentAttribute);
				autoRegisterFilterAttributes.Add(autoRegisterFilterAttributeData);
				type.CustomAttributes.RemoveAt(i);
			}
		}

		if (autoRegisterAttributes.Count + autoRegisterFilterAttributes.Count == 0)
			return;

		MethodDefinition registerServicesMethod =
			type
			.Methods
			.Single(x =>
				x.Name == "RegisterServices"
				&& x.IsPublic
				&& x.IsStatic
				&& x.ReturnType.FullName == "System.Void"
				&& x.Parameters.Count == 1
				&& x.Parameters[0].ParameterType.FullName == "Microsoft.Extensions.DependencyInjection.IServiceCollection"
			);

		ILProcessor ilProcessor = registerServicesMethod.Body.GetILProcessor();
		registerServicesMethod.Body.Instructions.Clear();

		IEnumerable<TypeDefinition> filteredClasses =
			classesToScan
			.Where(c => autoRegisterFilterAttributes.All(f => f.Matches(c)));

		manifestBuilder.AppendLinuxLine($"{type.ToHumanReadableName()}");
		foreach (AutoRegisterAttributeData autoRegisterAttributeData in autoRegisterAttributes)
			ProcessAutoRegisterAttribute(
				manifestBuilder: manifestBuilder,
				filteredClasses: filteredClasses,
				autoRegisterAttributeData: autoRegisterAttributeData,
				ilProcessor: ilProcessor);

		AddCallToAfterRegisterServices(type, ilProcessor);

		ilProcessor.Emit(OpCodes.Ret);
	}

	private static void AddCallToAfterRegisterServices(
		TypeDefinition type,
		ILProcessor ilProcessor)
	{
		MethodDefinition? afterRegisterServicesMethod =
			type
			.Methods
			.SingleOrDefault(x =>
				x.Name == "AfterRegisterServices"
				&& x.IsStatic
				&& x.ReturnType.FullName == "System.Void"
				&& x.Parameters.Count == 1
				&& x.Parameters[0].ParameterType.FullName == "Microsoft.Extensions.DependencyInjection.IServiceCollection"
			);
		if (afterRegisterServicesMethod is not null)
		{
			ilProcessor.Emit(OpCodes.Ldarg_0);
			ilProcessor.Emit(OpCodes.Call, afterRegisterServicesMethod);
		}
	}

	private void ProcessAutoRegisterAttribute(
		StringBuilder manifestBuilder,
		IEnumerable<TypeDefinition> filteredClasses,
		AutoRegisterAttributeData autoRegisterAttributeData,
		ILProcessor ilProcessor)
	{
		manifestBuilder.Append(",");
		manifestBuilder.Append($"Find {autoRegisterAttributeData.Find}");
		manifestBuilder.Append($" {autoRegisterAttributeData.Type.ToHumanReadableName()}");
		manifestBuilder.Append($" RegisterAs {autoRegisterAttributeData.Register}");

		if (autoRegisterAttributeData.ServiceTypeFilter is not null)
			manifestBuilder.Append($" ServiceTypeFilter=\"{autoRegisterAttributeData.ServiceTypeFilter}\"");

		if (autoRegisterAttributeData.ServiceImplementationFilter is not null)
			manifestBuilder.Append($" ServiceImplementationFilter=\"{autoRegisterAttributeData.ServiceImplementationFilter}\"");

		manifestBuilder.AppendLinuxLine();

		foreach (TypeDefinition candidate in filteredClasses)
		{
			if (autoRegisterAttributeData.IsMatch(candidate, out TypeReference? serviceType))
				RegisterClass(
					manifestBuilder: manifestBuilder,
					withLifetime: autoRegisterAttributeData.WithLifetime,
					serviceType: serviceType,
					serviceImplementationType: candidate,
					ilProcessor: ilProcessor);
		}
	}

	private void RegisterClass(
		StringBuilder manifestBuilder,
		WithLifetime withLifetime,
		TypeReference? serviceType,
		TypeDefinition serviceImplementationType,
		ILProcessor ilProcessor)
	{
		manifestBuilder.Append(",,");
		manifestBuilder.Append($"{withLifetime},");
		manifestBuilder.Append($"{serviceType!.ToHumanReadableName()},");
		manifestBuilder.Append($"{serviceImplementationType!.ToHumanReadableName()}");
		manifestBuilder.AppendLinuxLine();

		// Get references to needed runtime methods
		var getTypeFromHandleRef = ModuleDefinition.ImportReference(
			typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new[] { typeof(RuntimeTypeHandle) })
		);

		// Determine which AddXxx method to call
		var extensionType = typeof(ServiceCollectionServiceExtensions);
		var addMethodName = withLifetime switch {
			WithLifetime.Singleton => "AddSingleton",
			WithLifetime.Scoped => "AddScoped",
			WithLifetime.Transient => "AddTransient",
			_ => throw new InvalidOperationException("Unsupported lifetime")
		};

		// Import the chosen extension method
		var addMethodRef = ModuleDefinition.ImportReference(
			extensionType.GetMethod(addMethodName, new[] { typeof(IServiceCollection), typeof(Type), typeof(Type) })
		);

		// Emit: services.AddXxx(typeof(ServiceType), typeof(ServiceImplementationType));
		ilProcessor.Emit(OpCodes.Ldarg_0);
		ilProcessor.Emit(OpCodes.Ldtoken, serviceType);
		ilProcessor.Emit(OpCodes.Call, getTypeFromHandleRef);
		ilProcessor.Emit(OpCodes.Ldtoken, serviceImplementationType);
		ilProcessor.Emit(OpCodes.Call, getTypeFromHandleRef);
		ilProcessor.Emit(OpCodes.Call, addMethodRef);
		ilProcessor.Emit(OpCodes.Pop);
	}

	private void WriteManifestFile(string content)
	{
		string manifestFilePath = Path.ChangeExtension(ProjectFilePath, "Morris.AutoRegister.manifest");
		File.WriteAllText(manifestFilePath, content);
	}

	private void RemoveDependency()
	{
		var assemblyReference =
			ModuleDefinition
			.AssemblyReferences
			.FirstOrDefault(x => x.Name == "Morris.AutoRegister");

		if (assemblyReference is not null)
			ModuleDefinition.AssemblyReferences.Remove(assemblyReference);
	}
}
