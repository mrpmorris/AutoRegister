using Mono.Cecil;
using System.Collections.Generic;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class ModuleDefinitionGetAllTypesExtension
{
	public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition module)
	{
		foreach (TypeDefinition type in module.Types)
		{
			yield return type;
			foreach (TypeDefinition nestedType in GetTypeAndNestedTypes(type))
				yield return nestedType;
		}

		IEnumerable<TypeDefinition> GetTypeAndNestedTypes(TypeDefinition type)
		{
			yield return type;
			foreach (TypeDefinition nestedType in type.NestedTypes)
			{
				foreach (TypeDefinition inner in GetTypeAndNestedTypes(nestedType))
					yield return inner;
			}
		}
	}
}
