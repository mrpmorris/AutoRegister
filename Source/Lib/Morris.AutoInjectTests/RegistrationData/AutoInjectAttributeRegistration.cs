using Morris.AutoInject;
using Morris.AutoInject.TestsShared;
using System.Text;

namespace Morris.AutoInjectTests.RegistrationData;

internal class AutoInjectAttributeRegistration
{
	public Find Find { get; private set; }
	public RegisterAs RegisterAs { get; private set; }
	public string? ServiceIdentifierFilter { get; private set; }
	public string? ServiceImplementationFilter { get; private set; }
	public string TypeFullName { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	public AutoInjectAttributeRegistration(
		Find find,
		string typeFullName,
		RegisterAs registerAs,
		WithLifetime withLifetime,
		string? serviceIdentifierFilter = null,
		string? serviceImplementationFilter = null)
	{
		Find = find;
		RegisterAs = registerAs;
		ServiceIdentifierFilter = serviceIdentifierFilter;
		ServiceImplementationFilter = serviceImplementationFilter;
		TypeFullName = typeFullName;
		WithLifetime = withLifetime;
	}

	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"Find {Find}");
		builder.Append($" {TypeFullName}");
		builder.Append($" RegisterAs {RegisterAs}");

		if (ServiceIdentifierFilter is not null)
			builder.Append($" ServiceIdentifierFilter=\"{ServiceIdentifierFilter}\"");

		if (ServiceImplementationFilter is not null)
			builder.Append($" ServiceIdentifierFilter=\"{ServiceImplementationFilter}\"");
		
		return builder.ToString();
	}
}
