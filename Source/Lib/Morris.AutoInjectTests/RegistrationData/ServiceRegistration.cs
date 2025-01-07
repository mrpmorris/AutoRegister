using Microsoft.Extensions.DependencyInjection;

namespace Morris.AutoInjectTests.RegistrationData;

internal class ServiceRegistration
{
	public ServiceLifetime Lifetime { get; private set; }
	public string ServiceTypeFullName {  get; private set; }
	public string ServiceImplementationTypeFullName {  get; private set; }

	public ServiceRegistration(
		ServiceLifetime lifetime,
		string serviceTypeFullName,
		string serviceImplementationTypeFullName)
	{
		Lifetime = lifetime;
		ServiceTypeFullName = serviceTypeFullName;
		ServiceImplementationTypeFullName = serviceImplementationTypeFullName;
	}

	public override string ToString() =>
		$"{Lifetime},{ServiceTypeFullName},{ServiceImplementationTypeFullName}";
}
