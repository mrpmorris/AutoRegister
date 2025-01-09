using Mono.Cecil;
using System.Linq;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionGetGenericTypeExtension
{
	This doesn't work
	public static TypeDefinition? GetGenericType(this TypeDefinition baseGenericType, TypeReference[] parameters) =>
		baseGenericType
		.Module
		.Types
		.Where(x => 
			x.BaseType is GenericInstanceType genericInstanceType
			&& genericInstanceType.ElementType.Resolve() == baseGenericType
			&& genericInstanceType.GenericArguments.Count == parameters.Length
		)
		.Where(x =>
		{
			var genericInstanceType = (GenericInstanceType)x.BaseType;
			for (int i = 0; i < genericInstanceType.GenericArguments.Count; i++)
			{
				TypeReference genericArgument = genericInstanceType.GenericArguments[i];
				TypeReference parameter = parameters[i];

				if (genericArgument != parameter)
					return false;
			}
			return true;
		})
		.FirstOrDefault()
		?.Resolve();
}
