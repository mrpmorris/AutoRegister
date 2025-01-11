using Morris.AutoRegister.TestsShared;
using System.Text;

namespace Morris.AutoRegisterTests.RegistrationData;

internal class ModuleRegistration
{
	public string ClassFullName { get; private set; }
	public IEnumerable<AutoRegisterAttributeRegistration> AutoRegisterAttributes { get; private set; }
	public IEnumerable<ServiceRegistration> Services { get; private set; }

	public ModuleRegistration(
		string classFullName,
		IEnumerable<AutoRegisterAttributeRegistration> autoRegisterAttributes,
		IEnumerable<ServiceRegistration> services)
	{
		ClassFullName = classFullName;
		AutoRegisterAttributes = autoRegisterAttributes.ToArray();
		Services = services.ToArray();
	}

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder.AppendLine(ClassFullName);
		foreach (AutoRegisterAttributeRegistration attribute in AutoRegisterAttributes)
			builder.AppendLinuxLine($",{attribute.ToString()}");
		foreach (ServiceRegistration service in Services.OrderBy(x => x.ServiceTypeFullName))
			builder.AppendLinuxLine($",,{service.ToString()}");
		return builder.ToString();
	}
}
