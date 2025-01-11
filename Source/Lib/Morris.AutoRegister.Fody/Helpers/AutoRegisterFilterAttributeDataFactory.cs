using Mono.Cecil;
using System;

namespace Morris.AutoRegister.Fody.Helpers;

internal static class AutoRegisterFilterAttributeDataFactory
{
	private static int? ServiceImplementationFilterParameterIndex;

	public static AutoRegisterFilterAttributeData Create(CustomAttribute attr)
	{
		if (ServiceImplementationFilterParameterIndex is null)
			ResolveParameterIndexes(attr);

		var serviceImplementationRegex = (string?)attr.ConstructorArguments[ServiceImplementationFilterParameterIndex!.Value].Value;

		foreach (var namedArg in attr.Properties)
			throw new NotImplementedException($"Unexpected parameter \"{namedArg.Name}\"");

		var result = new AutoRegisterFilterAttributeData(serviceImplementationRegex: serviceImplementationRegex);

		return result;
	}

	private static void ResolveParameterIndexes(CustomAttribute attr)
	{
		var parameters = attr.Constructor.Resolve().Parameters;

		for (int i = 0; i < parameters.Count; i++)
		{
			ParameterDefinition parameter = parameters[i];
			switch (parameter.Name)
			{
				case "serviceImplementationFilter":
					ServiceImplementationFilterParameterIndex = i;
					break;

				default:
					throw new NotImplementedException($"Unexpected parameter \"{parameter.Name}\".");
			}
		}

		if (ServiceImplementationFilterParameterIndex is null)
			throw new InvalidOperationException("Parameter \"serviceImplementationRegex\" was not found.");
	}
}
