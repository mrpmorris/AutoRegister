using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class TypeDefinitionGetAllInterfacesExtension
{
	public static IEnumerable<TypeReference> GetAllInterfaces(this TypeReference typeReference)
	{
		var result = new HashSet<TypeReference>();

		TypeReference? current = typeReference;
		while (current is not null)
		{
			TypeDefinition currentDefinition = current.Resolve();

			foreach (InterfaceImplementation interfaceImplementation in currentDefinition.Interfaces)
			{
				TypeReference interfaceType = interfaceImplementation.InterfaceType;
				if (current is GenericInstanceType genericInstance)
					interfaceType = SubstituteGenericArgs(interfaceType, currentDefinition, genericInstance);
				result.Add(interfaceType);
			}

			TypeReference? baseType = currentDefinition.BaseType;
			if (baseType is not null && current is GenericInstanceType currentGeneric)
				baseType = SubstituteGenericArgs(baseType, currentDefinition, currentGeneric);
			current = baseType;
		}
		return result;
	}

	private static TypeReference SubstituteGenericArgs(
		TypeReference typeReference,
		TypeDefinition declaringDefinition,
		GenericInstanceType genericInstance)
	{
		if (typeReference is not GenericInstanceType genericType)
			return typeReference;

		var resolved = new GenericInstanceType(genericType.ElementType);
		foreach (TypeReference argument in genericType.GenericArguments)
		{
			if (argument is not GenericParameter genericParameter)
				resolved.GenericArguments.Add(argument);
			else
			{
				int index = declaringDefinition
					.GenericParameters
					.Select((parameter, parameterIndex) => (parameter, parameterIndex))
					.Where(x => x.parameter.Name == genericParameter.Name)
					.Select(x => x.parameterIndex)
					.DefaultIfEmpty(-1)
					.First();
				TypeReference argumentToAdd =
					index >= 0 && index < genericInstance.GenericArguments.Count
					? genericInstance.GenericArguments[index]
					: argument;
				resolved.GenericArguments.Add(argumentToAdd);
			}
		}
		return resolved;
	}
}
