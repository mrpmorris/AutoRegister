using Mono.Cecil;
using System.Linq;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class TypeDefinitionToHumanReadableNameExtension
{
	public static string ToHumanReadableName(this TypeReference typeReference)
	{
		if (typeReference is RequiredModifierType requiredModifierType)
			return requiredModifierType.ElementType.ToHumanReadableName();
		
		if (typeReference is OptionalModifierType optionalModifierType)
			return optionalModifierType.ElementType.ToHumanReadableName();
		
		if (typeReference.IsGenericInstance && typeReference.FullName.StartsWith("System.Nullable`1"))
		{
			TypeReference underlyingType = ((GenericInstanceType)typeReference).GenericArguments.First();
			return $"System.Nullable<{underlyingType.ToHumanReadableName()}>";
		}

		string name;
		if (typeReference is GenericInstanceType genericType)
		{
			string genericTypeName = genericType.ElementType.Name;
			int backtickIndex = genericTypeName.IndexOf('`');

			if (backtickIndex > 0)
				genericTypeName = genericTypeName.Substring(0, backtickIndex);

			var genericArguments =
				genericType
				.GenericArguments
				.Select(arg => arg.ToHumanReadableName());

			name = $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
		}
		else if (typeReference.IsArray)
		{
			var arrayType = (ArrayType)typeReference;
			string elementType = arrayType.ElementType.ToHumanReadableName();
			int rank = arrayType.Rank;

			// Handle multi-dimensional arrays
			string rankSpecifier = rank == 1 ? "[]" : $"[{new string(',', rank - 1)}]";
			name = $"{elementType}{rankSpecifier}";
		}
		else
			name = typeReference.Name;

		if (typeReference.DeclaringType is not null)
			return $"{typeReference.DeclaringType.ToHumanReadableName()}+{name}";

		if (typeReference.Namespace is null)
			return name;
	
		return $"{typeReference.Namespace}.{name}";
	}
}
