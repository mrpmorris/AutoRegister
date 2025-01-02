using Fody;
using System.Collections.Generic;
using System.IO;

namespace Morris.AutoInject.Fody;

internal class ModuleWeaver : BaseModuleWeaver
{
	public override IEnumerable<string> GetAssembliesForScanning()
	{
		return ["netstandard", "mscorlib"];
	}

	public override void Execute()
	{
		string manifestFilePath = Path.Combine(ProjectDirectoryPath, "autoinject.manifest");
		File.WriteAllText(manifestFilePath, "Hello");
	}

}
