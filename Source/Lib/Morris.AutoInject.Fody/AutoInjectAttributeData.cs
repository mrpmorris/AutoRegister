using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Morris.AutoInject.Fody;

internal class AutoInjectAttributeData
{
	public Find Find { get; private set; }
	public RegisterAs RegisterAs { get; private set; }
	public string? ServiceIdentifierRegex { get; private set; }
	public string? ServiceImplementationRegex { get; private set; }
	public TypeReference Type { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	public AutoInjectAttributeData(
		Find find,
		RegisterAs registerAs,
		string? serviceIdentifierRegex,
		string? serviceImplementationRegex,
		TypeReference type,
		WithLifetime withLifetime)
	{
		Find = find;
		RegisterAs = registerAs;
		ServiceIdentifierRegex = serviceIdentifierRegex;
		ServiceImplementationRegex = serviceImplementationRegex;
		Type = type;
		WithLifetime = withLifetime;
	}

}
