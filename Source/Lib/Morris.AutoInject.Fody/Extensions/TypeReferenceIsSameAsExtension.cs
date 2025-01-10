using Mono.Cecil;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeReferenceIsSameAsExtension
{
	public static bool IsSameAs(this TypeReference? first, TypeReference? second)
	{
		if (first is null && second is null)
			return true;
		if (first is null || second is null)
			return false;
		if (first.FullName == second.FullName)
			return true;

		if (first is GenericInstanceType giLeft)
		{
			if (second is GenericInstanceType giRight)
				return giLeft.ElementType.FullName == giRight.ElementType.FullName;
			if (second.HasGenericParameters)
				return giLeft.ElementType.FullName == second.FullName;
		}
		return false;
	}

}
