using Fody;
using Mono.Cecil;
using Morris.AutoInject.Fody.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
		IEnumerable<TypeDefinition> classes = ModuleDefinition.Types.Where(x => x.IsClass);
		foreach (var type in classes)
			ScanType(type);

		WriteManifestFile();
	}

	private void ScanType(TypeDefinition type)
	{
		CustomAttribute[] autoInjectAttributes =
			type
			.CustomAttributes
			.Where(x => x.AttributeType.FullName == "Morris.AutoInject.AutoInjectAttribute")
			.ToArray();

		CustomAttribute[] autoInjectFilterAttributes =
			type
			.CustomAttributes
			.Where(x => x.AttributeType.FullName == "Morris.AutoInject.AutoInjectFilterAttribute")
			.ToArray();

		if (autoInjectAttributes.Length + autoInjectFilterAttributes.Length == 0)
			return;

		foreach(var attribute in autoInjectAttributes)
		{
			var x = attribute.GetValues();
		}
	}



	private void WriteManifestFile()
	{
		string manifestFilePath = Path.ChangeExtension(ProjectFilePath, "Morris.AutoInject.manifest");
		File.WriteAllText(manifestFilePath, "Hello");
	}
}
