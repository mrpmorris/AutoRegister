using Mono.Cecil;
using System.Collections.Generic;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionGetAllInterfacesExtension
{
	public static IEnumerable<TypeReference> GetAllInterfaces(this TypeReference typeReference)
	{
		var visited = new HashSet<TypeDefinition>();

		TypeDefinition? currentType = typeReference.Resolve();
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
