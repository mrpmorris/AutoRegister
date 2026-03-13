using Mono.Cecil;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class TypeReferenceIsAssignableToExtension
{
	public static bool IsAssignableTo(this TypeReference candidate, TypeReference target) =>
		candidate.IsSameAs(target)
		|| candidate.DescendsFrom(target);
}
