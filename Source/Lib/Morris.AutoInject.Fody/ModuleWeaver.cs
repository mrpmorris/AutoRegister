using Fody;
using Mono.Cecil;
using Morris.AutoInject.Fody.Extensions;
using System;
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
		var manifestBuilder = new StringBuilder();

		IEnumerable<TypeDefinition> classesToScan =
			ModuleDefinition
			.Types
			.Where(x => x.IsClass)
			.OrderBy(x => x.FullName);

		foreach (TypeDefinition type in classesToScan)
			ScanType(type, manifestBuilder);

		WriteManifestFile(manifestBuilder.ToString());
	}

	private void ScanType(TypeDefinition type, StringBuilder manifestBuilder)
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

		manifestBuilder.Append($"{type.FullName}\n");
		foreach(var attribute in autoInjectAttributes)
		{
			var x = attribute.GetValues();
		}
		manifestBuilder.Append("\n");
	}



	private void WriteManifestFile(string content)
	{
		string manifestFilePath = Path.ChangeExtension(ProjectFilePath, "Morris.AutoInject.manifest");
		File.WriteAllText(manifestFilePath, content);
	}
}
