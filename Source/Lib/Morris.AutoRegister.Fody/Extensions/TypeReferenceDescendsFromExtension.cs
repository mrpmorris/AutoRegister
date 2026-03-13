using Mono.Cecil;
using System.Linq;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class TypeReferenceDescendsFromExtension
{
	public static bool DescendsFrom(this TypeReference child, TypeReference baseType)
	{
		TypeDefinition childDefinition = child.Resolve();

		if (childDefinition.IsClass)
		{
			TypeReference? current = childDefinition.BaseType;
			while (current is not null)
			{
				if (current.IsSameAs(baseType))
					return true;

				current = current.Resolve().BaseType;
			}
		}
		else if (childDefinition.IsInterface)
		{
			if (!child.IsSameAs(baseType) && childDefinition.Interfaces.Any(x => x.InterfaceType.IsSameAs(baseType)))
				return true;
		}

		return false;
	}
}
