using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class CustomAttributeGetValuesExtension
{
	public static Dictionary<string, object?> GetValues(this CustomAttribute attr)
	{
		var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
		var constructorParams = attr.Constructor.Resolve().Parameters;

		for (int i = 0; i < attr.ConstructorArguments.Count; i++)
			result[constructorParams[i].Name] = attr.ConstructorArguments[i].Value;

		foreach (CustomAttributeNamedArgument namedArg in attr.Properties)
			result[namedArg.Name] = namedArg.Argument.Value;

		return result;
	}
}
