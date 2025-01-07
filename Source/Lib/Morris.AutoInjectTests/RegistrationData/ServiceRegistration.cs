using Microsoft.Extensions.DependencyInjection;

namespace Morris.AutoInjectTests.RegistrationData;

internal class ServiceRegistration
{
	public ServiceLifetime Lifetime { get; private set; }
	public string ServiceIdentifierFullName {  get; private set; }
	public string ServiceImplementorFullName {  get; private set; }

	public ServiceRegistration(
		ServiceLifetime lifetime,
		string serviceIdentifierFullName,
		string serviceImplementorFullName)
	{
		Lifetime = lifetime;
		ServiceIdentifierFullName = serviceIdentifierFullName;
		ServiceImplementorFullName = serviceImplementorFullName;
	}

	public override string ToString() =>
		$"{Lifetime},{ServiceIdentifierFullName},{ServiceImplementorFullName}";
}
