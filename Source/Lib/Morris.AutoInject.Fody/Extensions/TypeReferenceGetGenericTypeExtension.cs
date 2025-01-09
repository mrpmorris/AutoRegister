using Mono.Cecil;

namespace Morris.AutoInject.Fody.Extensions;

internal static class TypeReferenceGetGenericTypeExtension
{
	public static TypeReference GetGenericType(this TypeReference baseGenericType, TypeReference[] parameters)
	{
		var instance = new GenericInstanceType(baseGenericType);
		foreach (var p in parameters)
			instance.GenericArguments.Add(p);

		return instance;
	}
}
