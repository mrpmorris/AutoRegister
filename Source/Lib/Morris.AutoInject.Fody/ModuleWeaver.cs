using Fody;
using Mono.Cecil;
using Morris.AutoInject.Fody.Extensions;
using Morris.AutoInject.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Morris.AutoInject.Fody;

public class ModuleWeaver : BaseModuleWeaver
{
	public const string ManifestHeader = "Module,Attribute,Scope,ServiceIdentifier,ServiceImplementation";

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
			.Types
			.Where(x => x.IsClass)
			.Where(x => !x.HasGenericParameters)
			.Where(x => !x.IsAbstract)
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
		var autoInjectAttributes = new List<AutoInjectAttributeData>();
		var autoInjectFilterAttributes = new List<AutoInjectFilterAttributeData>();
		for (int i = type.CustomAttributes.Count - 1; i >= 0; i--)
		{
			CustomAttribute currentAttribute = type.CustomAttributes[i];
			if (currentAttribute.AttributeType.FullName == "Morris.AutoInject.AutoInjectAttribute")
			{
				AutoInjectAttributeData autoInjectAttributeData = AutoInjectAttributeDataFactory.Create(currentAttribute);
				autoInjectAttributes.Add(autoInjectAttributeData);
				type.CustomAttributes.RemoveAt(i);
			}
			else if (currentAttribute.AttributeType.FullName == "Morris.AutoInject.AutoInjectFilterAttribute")
			{
				AutoInjectFilterAttributeData autoInjectFilterAttributeData = AutoInjectFilterAttributeDataFactory.Create(currentAttribute);
				autoInjectFilterAttributes.Add(autoInjectFilterAttributeData);
				type.CustomAttributes.RemoveAt(i);
			}
		}

		if (autoInjectAttributes.Count + autoInjectFilterAttributes.Count == 0)
			return;

		//MethodDefinition registerServicesMethod = type
		//	.Methods
		//	.Single(
		//		x => x.Name == "RegisterServices"
		//		&& x.IsPublic
		//		&& x.IsStatic
		//		&& x.ReturnType.FullName == "System.Void"
		//		&& x.Parameters.Count == 1
		//		&& x.Parameters[0].ParameterType.FullName == "Microsoft.Extensions.DependencyInjection.IServiceCollection"
		//	);

		IEnumerable<TypeDefinition> filteredClasses =
			classesToScan
			.Where(c => autoInjectFilterAttributes.All(f => f.Matches(c)));

		manifestBuilder.AppendLinuxLine($"{type.FullName}");
		foreach (AutoInjectAttributeData autoInjectAttributeData in autoInjectAttributes)
			ProcessAutoInjectAttribute(manifestBuilder, filteredClasses, autoInjectAttributeData);
	}

	private void ProcessAutoInjectAttribute(
		StringBuilder manifestBuilder,
		IEnumerable<TypeDefinition> filteredClasses,
		AutoInjectAttributeData autoInjectAttributeData)
	{
		manifestBuilder.Append(",");
		manifestBuilder.Append($"Find {autoInjectAttributeData.Find}");
		manifestBuilder.Append($" {autoInjectAttributeData.Type.FullName}");
		manifestBuilder.Append($" RegisterAs {autoInjectAttributeData.RegisterAs}");

		if (autoInjectAttributeData.ServiceIdentifierFilter is not null)
			manifestBuilder.Append($" ServiceIdentifierFilter=\"{autoInjectAttributeData.ServiceIdentifierFilter}\"");

		if (autoInjectAttributeData.ServiceImplementationFilter is not null)
			manifestBuilder.Append($" ServiceIdentifierFilter=\"{autoInjectAttributeData.ServiceImplementationFilter}\"");

		manifestBuilder.AppendLinuxLine();

		foreach (TypeDefinition candidate in filteredClasses)
		{
			if (autoInjectAttributeData.IsMatch(candidate, out TypeReference? serviceIdentifier))
				RegisterClass(manifestBuilder, autoInjectAttributeData.WithLifetime, serviceIdentifier, candidate);
		}
	}

	private void RegisterClass(
		StringBuilder manifestBuilder,
		WithLifetime withLifetime,
		TypeReference? serviceIdentifier,
		TypeDefinition serviceImplementer)
	{
		manifestBuilder.Append(",,");
		manifestBuilder.Append($"{withLifetime},");
		manifestBuilder.Append($"{serviceIdentifier!.FullName},");
		manifestBuilder.Append($"{serviceImplementer!.FullName}");
		manifestBuilder.AppendLinuxLine();
	}

	private void WriteManifestFile(string content)
	{
		string manifestFilePath = Path.ChangeExtension(ProjectFilePath, "Morris.AutoInject.manifest");
		File.WriteAllText(manifestFilePath, content);
	}

	private void RemoveDependency()
	{
		var assemblyReference =
			ModuleDefinition
			.AssemblyReferences
			.FirstOrDefault(x => x.Name == "Morris.AutoInject");

		if (assemblyReference is not null)
			ModuleDefinition.AssemblyReferences.Remove(assemblyReference);
	}
}
