using Fody;
using Mono.Cecil;
using Morris.AutoInject.Fody.Extensions;
using Morris.AutoInject.Fody.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Morris.AutoInject.Fody;

public class ModuleWeaver : BaseModuleWeaver
{
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

		IEnumerable<TypeDefinition> classesToScan =
			ModuleDefinition
			.Types
			.Where(x => x.IsClass)
			.Where(x => !x.HasGenericParameters)
			.Where(x => !x.IsAbstract)
			.OrderBy(x => x.FullName);

		foreach (TypeDefinition type in classesToScan)
			ScanType(type, manifestBuilder);

		WriteManifestFile(manifestBuilder.ToString());
	}

	private void ScanType(TypeDefinition type, StringBuilder manifestBuilder)
	{
		var autoInjectAttributes = new List<AutoInjectAttributeData>();
		var autoInjectFilterAttributes = new List<CustomAttribute>();
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
				autoInjectFilterAttributes.Add(currentAttribute);
				type.CustomAttributes.RemoveAt(i);
			}
		}

		if (autoInjectAttributes.Count + autoInjectFilterAttributes.Count == 0)
			return;

		manifestBuilder.Append($"{type.FullName}\n");
		manifestBuilder.Append("\n");
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
