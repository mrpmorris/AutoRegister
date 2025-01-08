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

		string? errorText = BuildErrorText(missingRegistrations, unexpectedRegistrations);
		Assert.IsTrue(errorText is null, errorText);

		string expectedManifest = BuildExpectedManifest(expectedModuleRegistrations);
		Assert.AreEqual(expectedManifest.StandardizeLines(), manifest.StandardizeLines());
	}

	private static string? BuildErrorText(
		List<ServiceRegistration> missingRegistrations,
		List<ServiceRegistration> unexpectedRegistrations)
	{
		if (missingRegistrations.Count + unexpectedRegistrations.Count == 0)
			return null;

		var builder = new StringBuilder();
		AddRegistrationsErrorText(builder, "Missing registrations", missingRegistrations);
		AddRegistrationsErrorText(builder, "Unexpected registrations", unexpectedRegistrations);

		return builder.ToString();
	}

	private static void AddRegistrationsErrorText(
		StringBuilder builder,
		string header,
		List<ServiceRegistration> registrations)
	{
		if (registrations.Count == 0)
			return;

		builder.AppendLine();
		builder.AppendLine(header);
		builder.AppendLine(new string('=', header.Length));
		foreach(ServiceRegistration registration in registrations)
			builder.AppendLine($"{registration.Lifetime},{registration.ServiceTypeFullName},{registration.ServiceImplementationTypeFullName}");
	}

	private static void CheckModuleRegistrations(
		Assembly assembly,
		ModuleRegistration module,
		List<ServiceRegistration> missingRegistrations,
		List<ServiceRegistration> unexpectedRegistrations)
	{
		Type moduleType = assembly.GetType(module.ClassFullName)!;
		Assert.IsNotNull(moduleType, $"Module not found \"{module.ClassFullName}\".");

		MethodInfo registerServicesMethod =
			moduleType
			.GetMethod("RegisterServices", BindingFlags.Public | BindingFlags.Static)!;
		Assert.IsNotNull(registerServicesMethod, $"{module.ClassFullName}.RegisterServices method not found.");

		var services = new ServiceCollection();
		registerServicesMethod.Invoke(
			obj: null,
			parameters: [services]
		);

		var matchedServiceDescriptors = new HashSet<ServiceDescriptor>();
		foreach(ServiceRegistration expectedService in module.Services)
		{
			Type serviceType = assembly.GetType(expectedService.ServiceTypeFullName)!;
			Type serviceImplementationType = assembly.GetType(expectedService.ServiceImplementationTypeFullName)!;

			ServiceDescriptor? registrationFound =
				services
				.FirstOrDefault(x =>
					x.ServiceType == serviceType
					&& x.ImplementationType == serviceImplementationType
					&& x.Lifetime == expectedService.Lifetime
				);
			if (registrationFound is null)
				missingRegistrations.Add(expectedService);
			else
				matchedServiceDescriptors.Add(registrationFound);
		}

		IEnumerable<ServiceDescriptor> unexpectedServiceDescriptors = services.Except(matchedServiceDescriptors);
		foreach(ServiceDescriptor unexpectedServiceDescriptor in unexpectedServiceDescriptors)
		{
			var unexpectedRegistration = new ServiceRegistration(
				lifetime: unexpectedServiceDescriptor.Lifetime,
				serviceTypeFullName: unexpectedServiceDescriptor.ServiceType.FullName!,
				serviceImplementationTypeFullName: unexpectedServiceDescriptor.ImplementationType!.FullName!
			);
			unexpectedRegistrations.Add(unexpectedRegistration);
		}
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
