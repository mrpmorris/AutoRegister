using Mono.Cecil;
using System;

namespace Morris.AutoInject.Fody;

internal class AutoInjectAttributeData
{
	public Find Find { get; private set; }
	public RegisterAs RegisterAs { get; private set; }
	public string? ServiceTypeFilter { get; private set; }
	public string? ServiceImplementationFilter { get; private set; }
	public TypeReference Type { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	public AutoInjectAttributeData(
		Find find,
		RegisterAs registerAs,
		string? serviceTypeFilter,
		string? serviceImplementationFilter,
		TypeReference type,
		WithLifetime withLifetime)
	{
		Find = find;
		RegisterAs = registerAs;
		ServiceTypeFilter = serviceTypeFilter;
		ServiceImplementationFilter = serviceImplementationFilter;
		Type = type;
		WithLifetime = withLifetime;
	}

	public bool IsMatch(
		TypeDefinition type,
		out TypeReference? serviceType)
	{
		serviceType = type;
		return true;
	}

}
