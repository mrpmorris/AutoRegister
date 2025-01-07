using Mono.Cecil;
using System;

namespace Morris.AutoInject.Fody;

internal class AutoInjectAttributeData
{
	public Find Find { get; private set; }
	public RegisterAs RegisterAs { get; private set; }
	public string? ServiceIdentifierFilter { get; private set; }
	public string? ServiceImplementationFilter { get; private set; }
	public TypeReference Type { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	public AutoInjectAttributeData(
		Find find,
		RegisterAs registerAs,
		string? serviceIdentifierFilter,
		string? serviceImplementationFilter,
		TypeReference type,
		WithLifetime withLifetime)
	{
		Find = find;
		RegisterAs = registerAs;
		ServiceIdentifierFilter = serviceIdentifierFilter;
		ServiceImplementationFilter = serviceImplementationFilter;
		Type = type;
		WithLifetime = withLifetime;

	}

	public bool IsMatch(
		TypeDefinition type,
		out TypeReference? serviceIdentifier)
	{
		serviceIdentifier = type;
		return true;
	}

}
