using Mono.Cecil;
using System.Collections.Generic;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class ModuleDefinitionGetAllTypesExtension
{
	public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition module)
	{
		var result = new List<TypeDefinition>();
		foreach (TypeDefinition type in module.Types)
		{
			result.Add(type);
			AddNestedTypes(result, type);
		}
		return result;

		void AddNestedTypes(List<TypeDefinition> result, TypeDefinition type)
		{
			foreach (TypeDefinition nestedType in type.NestedTypes)
			{
				result.Add(nestedType);
				AddNestedTypes(result, nestedType);
			}
		}
	}
}
