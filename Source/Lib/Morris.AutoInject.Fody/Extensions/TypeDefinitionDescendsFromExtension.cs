using Mono.Cecil;
using System.Linq;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeDefinitionDescendsFromExtension
{
	public static bool DescendsFrom(this TypeDefinition child, TypeDefinition baseType)
	{
		if (child.IsClass)
		{
			TypeDefinition? current = child.BaseType?.Resolve();
			while (current is not null)
			{
				if (current.IsSameAs(baseType))
					return true;

				current = current.BaseType?.Resolve();
			}
		}
		else if (child.IsInterface)
		{
			if (child != baseType && child.GetAllInterfaces().Any(x => x.IsSameAs(baseType)))
				return true;
		}

		return false;
	}
}
