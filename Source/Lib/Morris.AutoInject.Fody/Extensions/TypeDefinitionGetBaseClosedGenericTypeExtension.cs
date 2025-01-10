using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionGetBaseClosedGenericTypeExtension
{
	public static TypeReference? GetBaseClosedGenericType(
		this TypeReference descendantType,
		TypeDefinition baseType)
	{
		if (!baseType.HasGenericParameters)
			return baseType;

		TypeReference[] types =
			baseType.IsInterface
			? FindInterfacePath(descendantType, baseType)?.ToArray()!
			: FindClassPath(descendantType, baseType).ToArray();
		if (types.Length == 0)
			return null;

		string baseTypeGenericName = baseType.FullName.Split('<')[0];

		var values = new Dictionary<string, TypeReference>();
		for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
		{
			TypeReference current = types[typeIndex];
			TypeDefinition currentTypeDefinition = current.Resolve();

			if (current is not GenericInstanceType genericType)
				values.Clear();
			else
			{
				for (int genericArgumentIndex = 0; genericArgumentIndex < genericType.GenericArguments.Count; genericArgumentIndex++)
				{
					string name = currentTypeDefinition.GenericParameters[genericArgumentIndex].Name;
					TypeReference currentGenericArgument = genericType.GenericArguments[genericArgumentIndex];
					if (currentGenericArgument is GenericParameter genericParameter)
						values[name] = values[genericParameter.Name];
					else
						values[name] = currentGenericArgument;
				}
			}
		}

		TypeReference[] parameters =
			baseType
			.GenericParameters
			.Select(x => values[x.Name])
			.ToArray();
		return baseType.GetGenericType(parameters);
	}

	private static IEnumerable<TypeReference> FindClassPath(TypeReference descendantClass, TypeReference baseClass)
	{
		var result = new List<TypeReference>();
		TypeReference? current = descendantClass;
		while (current is not null)
		{
			result.Add(current);
			if (current.IsSameAs(baseClass))
				return result;
			current = current.Resolve().BaseType;
		}
		return Array.Empty<TypeReference>();
	}

	private static IEnumerable<TypeReference> FindInterfacePath(TypeReference? descendantInterface, TypeReference baseInterface) =>
		descendantInterface is null
		? Array.Empty<TypeReference>()
		: descendantInterface.IsSameAs(baseInterface)
		? [baseInterface]
		: descendantInterface
			.Resolve()
			.Interfaces!
			.Select(x => FindInterfacePath(x.InterfaceType, baseInterface))
			.Where(x => x.Any())
			.First()
			.Append(descendantInterface)
			.Reverse();
}