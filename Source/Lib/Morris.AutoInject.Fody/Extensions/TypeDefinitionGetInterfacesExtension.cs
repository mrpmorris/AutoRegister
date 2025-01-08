using Mono.Cecil;
using System.Collections.Generic;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionGetInterfacesExtension
{
	public static IEnumerable<TypeDefinition> GetInterfaces(this TypeDefinition typeDefinition)
	{
		var visited = new HashSet<TypeDefinition>();

		TypeDefinition? currentType = typeDefinition;
		while (currentType is not null)
		{
			foreach (var interfaceImplementation in currentType.Interfaces)
			{
				TypeDefinition interfaceTypeDef = interfaceImplementation.InterfaceType.Resolve();
				if (interfaceTypeDef is not null && visited.Add(interfaceTypeDef))
					yield return interfaceTypeDef;
			}

			currentType = currentType.BaseType?.Resolve();
		}
	}
}
