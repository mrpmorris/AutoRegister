using Morris.AutoInject.Fody;
using Morris.AutoInject.TestsShared;
using System.Text;

namespace Morris.AutoInjectTests.RegistrationData;

internal class ModuleRegistration
{
	public string ClassFullName { get; private set; }
	public IEnumerable<AutoInjectAttributeRegistration> AutoInjectAttributes { get; private set; }
	public IEnumerable<ServiceRegistration> Services { get; private set; }

	public ModuleRegistration(
		string classFullName,
		IEnumerable<AutoInjectAttributeRegistration> autoInjectAttributes,
		IEnumerable<ServiceRegistration> services)
	{
		ClassFullName = classFullName;
		AutoInjectAttributes = autoInjectAttributes.ToArray();
		Services = services.ToArray();
	}

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder.AppendLine(ClassFullName);
		foreach (AutoInjectAttributeRegistration attribute in AutoInjectAttributes)
			builder.AppendLinuxLine($",{attribute.ToString()}");
		foreach (ServiceRegistration service in Services.OrderBy(x => x.ServiceTypeFullName))
			builder.AppendLinuxLine($",,{service.ToString()}");
		return builder.ToString();
	}
}
