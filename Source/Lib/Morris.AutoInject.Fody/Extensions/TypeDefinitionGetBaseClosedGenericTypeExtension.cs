using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionGetBaseClosedGenericTypeExtension
{
	public static TypeDefinition? GetBaseClosedGenericType(
		this TypeReference descendantType,
		TypeDefinition baseType)
	{
		if (!baseType.HasGenericParameters)
			return baseType.Resolve();

		string baseTypeGenericName = baseType.FullName.Split('<')[0];

		if (baseType.IsClass)
		{
			var values = new Dictionary<string, TypeReference>();
			TypeReference? current = descendantType;
			while (current is not null)
			{
				TypeDefinition currentTypeDefinition = current.Resolve();
				TypeReference? baseTypeReference = currentTypeDefinition.BaseType;

				if (current is not GenericInstanceType genericType)
					values.Clear();
				else
				{
					for (int i = 0; i < genericType.GenericArguments.Count; i++)
					{
						string name = currentTypeDefinition.GenericParameters[i].Name;
						TypeReference currentGenericArgument = genericType.GenericArguments[i];
						if (currentGenericArgument is GenericParameter genericParameter)
						{
							values[name] = values[genericParameter.Name];
						}
						else
						{
							values[name] = currentGenericArgument;
						}
					}

					if (current.FullName.Split('<')[0] == baseTypeGenericName)
					{
						var parameters =
							baseType
							.GenericParameters
							.Select(x => values[x.Name])
							.ToArray();
						return baseType.GetGenericType(parameters);
					}
				}


				current = baseTypeReference;
			}
		}
		else if (baseType.IsInterface)
		{
			foreach (var currentInterface in baseType.Interfaces)
			{
				TypeDefinition resolved = currentInterface.InterfaceType.Resolve();
				if (resolved.FullName == baseType.FullName)
					return resolved;
			}
		}
		return null;
	}
}
