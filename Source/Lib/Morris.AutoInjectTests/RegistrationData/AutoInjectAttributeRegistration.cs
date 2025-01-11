using Morris.AutoRegister;
using Morris.AutoRegister.TestsShared;
using System.Text;

namespace Morris.AutoRegisterTests.RegistrationData;

internal class AutoRegisterAttributeRegistration
{
	public Find Find { get; private set; }
	public RegisterAs RegisterAs { get; private set; }
	public string? ServiceTypeFilter { get; private set; }
	public string? ServiceImplementationTypeFilter { get; private set; }
	public string TypeFullName { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	public AutoRegisterAttributeRegistration(
		Find find,
		string typeFullName,
		RegisterAs registerAs,
		WithLifetime withLifetime,
		string? serviceTypeFilter = null,
		string? serviceImplementationTypeFilter = null)
	{
		Find = find;
		RegisterAs = registerAs;
		ServiceTypeFilter = serviceTypeFilter;
		ServiceImplementationTypeFilter = serviceImplementationTypeFilter;
		TypeFullName = typeFullName;
		WithLifetime = withLifetime;
	}

	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"Find {Find}");
		builder.Append($" {TypeFullName}");
		builder.Append($" RegisterAs {RegisterAs}");

		if (ServiceTypeFilter is not null)
			builder.Append($" ServiceTypeFilter=\"{ServiceTypeFilter}\"");

		if (ServiceImplementationTypeFilter is not null)
			builder.Append($" ServiceImplementationTypeFilter=\"{ServiceImplementationTypeFilter}\"");
		
		return builder.ToString();
	}
}
