using Mono.Cecil;
using System.Linq;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class TypeDefinitionToHumanReadableNameExtension
{
	public static string ToHumanReadableName(this TypeReference typeReference) =>
		ToHumanReadableNameImpl(typeReference).Replace('/', '+');

	private static string ToHumanReadableNameImpl(TypeReference typeReference)
	{
		if (typeReference is GenericInstanceType genericType)
		{
			string genericTypeName = genericType.ElementType.FullName!;
			int backtickIndex = genericTypeName.IndexOf('`');

			if (backtickIndex > 0)
				genericTypeName = genericTypeName.Substring(0, backtickIndex);

			var genericArguments =
				genericType
				.GenericArguments
				.Select(arg => arg.ToHumanReadableName());

			return $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
		}

		if (typeReference.IsArray)
		{
			var arrayType = (ArrayType)typeReference;
			string elementType = arrayType.ElementType.ToHumanReadableName();
			int rank = arrayType.Rank;

			// Handle multi-dimensional arrays
			string rankSpecifier = rank == 1 ? "[]" : $"[{new string(',', rank - 1)}]";
			return $"{elementType}{rankSpecifier}";
		}

		if (typeReference is RequiredModifierType requiredModifierType)
			return requiredModifierType.ElementType.ToHumanReadableName();

		if (typeReference is OptionalModifierType optionalModifierType)
			return optionalModifierType.ElementType.ToHumanReadableName();

		// Handle nullable types
		if (
			typeReference.IsGenericInstance
			&& typeReference.FullName.StartsWith("System.Nullable`1"))
		{
			var underlyingType = ((GenericInstanceType)typeReference).GenericArguments.First();
			return $"System.Nullable<{underlyingType.ToHumanReadableName()}>";
		}

		return typeReference.FullName ?? typeReference.Name;
	}
}
