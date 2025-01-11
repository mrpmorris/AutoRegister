using Mono.Cecil;
using System;

namespace Morris.AutoRegister.Fody.Helpers;

internal static class AutoRegisterAttributeDataFactory
{
	private static int? FindParameterIndex;
	private static int? RegisterAsParameterIndex;
	private static int? TypeParameterIndex;
	private static int? WithlifetimeParameterIndex;

	public static AutoRegisterAttributeData Create(CustomAttribute attr)
	{
		if (FindParameterIndex is null)
			ResolveParameterIndexes(attr);

		var find = (Find)attr.ConstructorArguments[FindParameterIndex!.Value].Value;
		var type = (TypeReference)attr.ConstructorArguments[TypeParameterIndex!.Value].Value;
		var registerAs = (RegisterAs)attr.ConstructorArguments[RegisterAsParameterIndex!.Value].Value;
		var withLifetime = (WithLifetime)attr.ConstructorArguments[WithlifetimeParameterIndex!.Value].Value;

		string? serviceTypeFilter = null;
		string? serviceImplementationTypeFilter = null;

		foreach (var namedArg in attr.Properties)
		{
			if (namedArg.Name == nameof(AutoRegisterAttributeData.ServiceTypeFilter))
				serviceTypeFilter = (string?)namedArg.Argument.Value;
			else if (namedArg.Name == nameof(AutoRegisterAttribute.ServiceImplementationTypeFilter))
				serviceImplementationTypeFilter = (string?)namedArg.Argument.Value;
			else
				throw new NotImplementedException($"Unexpected parameter \"{namedArg.Name}\"");
		}

		var result = new AutoRegisterAttributeData(
			find: find,
			registerAs: registerAs,
			serviceTypeFilter: serviceTypeFilter,
			serviceImplementationTypeFilter: serviceImplementationTypeFilter,
			type: type,
			withLifetime: withLifetime);

		return result;
	}

	private static void ResolveParameterIndexes(CustomAttribute attr)
	{
		var parameters = attr.Constructor.Resolve().Parameters;

		for(int i = 0; i < parameters.Count; i++)
		{
			ParameterDefinition parameter = parameters[i];
			switch (parameter.Name)
			{
				case "find":
					FindParameterIndex = i;
					break;

				case "registerAs":
					RegisterAsParameterIndex = i;
					break;

				case "type":
					TypeParameterIndex = i;
					break;

				case "withLifetime":
					WithlifetimeParameterIndex = i;
					break;

				default:
					throw new NotImplementedException($"Unexpected parameter \"{parameter.Name}\".");
			}
		}

		if (FindParameterIndex is null)
			throw new InvalidOperationException("Parameter \"find\" was not found.");

		if (RegisterAsParameterIndex is null)
			throw new InvalidOperationException("Parameter \"registerAs\" was not found.");

		if (TypeParameterIndex is null)
			throw new InvalidOperationException("Parameter \"type\" was not found.");

		if (WithlifetimeParameterIndex is null)
			throw new InvalidOperationException("Parameter \"withLifetime\" was not found.");
	}
}
