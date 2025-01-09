using Mono.Cecil;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionGetBaseClosedGenericTypeExtension
{
	public static TypeDefinition? GetBaseClosedGenericType(
		this TypeDefinition descendantType,
		TypeDefinition baseType)
	{
		if (!baseType.HasGenericParameters)
			return baseType;

		if (baseType.IsClass)
		{
			TypeDefinition? current = descendantType;
			while (current is not null)
			{
				if (current.FullName == baseType.FullName)
					return current;

				current = current.BaseType?.Resolve();
			}
		}
		else if (baseType.IsInterface)
		{
			foreach (var currentInterface in descendantType.Interfaces)
			{
				TypeDefinition resolved = currentInterface.InterfaceType.Resolve();
				if (resolved.FullName == baseType.FullName)
					return resolved;
			}
		}
		return null;
	}
}
