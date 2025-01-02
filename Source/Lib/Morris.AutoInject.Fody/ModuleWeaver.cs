﻿using Fody;
using System.Collections.Generic;
using System.IO;

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
		string manifestFilePath = Path.Combine(ProjectDirectoryPath, "Morris.AutoInject.manifest");
		File.WriteAllText(manifestFilePath, "Hello");
	}

}
