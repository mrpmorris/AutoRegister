using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister.Fody;
using Morris.AutoRegister.TestsShared;
using Morris.AutoRegisterTests.Extensions;
using Morris.AutoRegisterTests.RegistrationData;
using System.Reflection;
using System.Text;

namespace Morris.AutoRegisterTests.Helpers;

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
		Assert.True(errorText is null, errorText);

		string expectedManifest = BuildExpectedManifest(expectedModuleRegistrations);
		Assert.Equal(expectedManifest.StandardizeLines(), manifest.StandardizeLines());
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
		Assert.True(moduleType is not null, $"Module not found \"{module.ClassFullName}\".");

		MethodInfo registerServicesMethod =
			moduleType
			.GetMethod("RegisterServices", BindingFlags.Public | BindingFlags.Static)!;
		Assert.True(registerServicesMethod is not null, $"{module.ClassFullName}.RegisterServices method not found.");

		var services = new ServiceCollection();
		registerServicesMethod.Invoke(
			obj: null,
			parameters: [services]
		);

		var matchedServiceDescriptors = new HashSet<ServiceDescriptor>();
		foreach(ServiceRegistration expectedService in module.Services)
		{
			ServiceDescriptor? registrationFound =
				services
				.FirstOrDefault(x =>
					x.ServiceType.ToHumanReadableName() == expectedService.ServiceTypeFullName
					&& x.ImplementationType!.ToHumanReadableName() == expectedService.ServiceImplementationTypeFullName
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
				serviceTypeFullName: unexpectedServiceDescriptor.ServiceType.ToHumanReadableName(),
				serviceImplementationTypeFullName: unexpectedServiceDescriptor.ImplementationType!.ToHumanReadableName()
			);
			unexpectedRegistrations.Add(unexpectedRegistration);
		}
	}

	private static string BuildExpectedManifest(IEnumerable<ModuleRegistration> expectedModuleRegistrations)
	{
		var builder = new StringBuilder();
		builder.AppendLine(ModuleWeaver.ManifestHeader);
		foreach (var module in expectedModuleRegistrations)
			builder.Append(module);
		return builder.ToString();
	}
}
