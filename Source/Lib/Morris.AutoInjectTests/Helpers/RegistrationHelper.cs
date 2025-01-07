using Microsoft.Extensions.DependencyInjection;
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
		var missingRegistrations = new List<ServiceRegistration>();
		var unexpectedRegistrations = new List<ServiceRegistration>();

		foreach(var module in expectedModuleRegistrations)
			CheckModuleRegistrations(
				assembly: assembly,
				module: module,
				missingRegistrations: missingRegistrations,
				unexpectedRegistrations: unexpectedRegistrations);

		string expectedManifest = BuildExpectedManifest(expectedModuleRegistrations);
		Assert.AreEqual(expectedManifest.StandardizeLines(), manifest.StandardizeLines());

	}

	private static void CheckModuleRegistrations(
		Assembly assembly,
		ModuleRegistration module,
		List<ServiceRegistration> missingRegistrations,
		List<ServiceRegistration> unexpectedRegistrations)
	{
		Type moduleType = assembly.GetType(module.ClassFullName)!;
		Assert.IsNotNull(moduleType, $"Module not found \"{module.ClassFullName}\".");

		var xxx = moduleType.GetMethods(BindingFlags.Public | BindingFlags.Static);
		MethodInfo registerServicesMethod =
			moduleType
			.GetMethod("RegisterServices", BindingFlags.Public | BindingFlags.Static)!;
		Assert.IsNotNull(registerServicesMethod, $"{module.ClassFullName}.RegisterServices method not found.");

		var services = new ServiceCollection();
		registerServicesMethod.Invoke(
			obj: null,
			parameters: [services]
		);
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
