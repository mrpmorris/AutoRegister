using Mono.Cecil;
using System.Collections.Generic;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionGetAllInterfacesExtension
{
	public static IEnumerable<TypeReference> GetAllInterfaces(this TypeReference typeReference)
	{
		var result = new HashSet<TypeReference>();

		TypeDefinition? currentType = typeReference.Resolve();
		while (currentType is not null)
		{
			foreach (InterfaceImplementation interfaceImplementation in currentType.Interfaces)
				result.Add(interfaceImplementation.InterfaceType);

			currentType = currentType.BaseType?.Resolve();
		}
		return result;
	}
}
