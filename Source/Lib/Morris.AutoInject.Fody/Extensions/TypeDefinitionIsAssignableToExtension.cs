using Mono.Cecil;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionIsAssignableToExtension
{
	public static bool IsAssignableTo(this TypeDefinition candidate, TypeReference target)
	{
		TypeDefinition? currentType = candidate;
		while (currentType is not null)
		{
			if (currentType.FullName == target.FullName)
				return true;

			currentType = currentType.BaseType?.Resolve();
		}

		foreach (var currentInterface in candidate.Interfaces)
		{
			if (currentInterface.InterfaceType.FullName == target.FullName)
				return true;
		}

		return false;
	}

}
