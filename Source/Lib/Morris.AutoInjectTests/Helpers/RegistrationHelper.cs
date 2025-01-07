using Morris.AutoInject.Fody;
using Morris.AutoInject.TestsShared;
using Morris.AutoInjectTests.RegistrationData;
using System.Reflection;
using System.Text;

namespace Morris.AutoInjectTests.Helpers;

internal static class RegistrationHelper
{
	public static void AssertRegistration(
		Assembly assembly,
		string? manifest,
		IEnumerable<ModuleRegistration> expectedModuleRegistrations)
	{
		string expectedManifest = BuildExpectedManifest(expectedModuleRegistrations);
		Assert.AreEqual(expectedManifest.StandardizeLines(), manifest.StandardizeLines());
	}

	private static string BuildExpectedManifest(IEnumerable<ModuleRegistration> expectedModuleRegistrations)
	{
		var builder = new StringBuilder();
		builder.AppendLinuxLine(ModuleWeaver.ManifestHeader);
		foreach (var module in expectedModuleRegistrations)
		{
			builder.Append(module);
		}
		return builder.ToString();
	}
}
