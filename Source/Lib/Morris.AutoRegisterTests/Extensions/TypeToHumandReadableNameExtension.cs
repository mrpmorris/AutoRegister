namespace Morris.AutoRegisterTests.Extensions;

internal static class TypeToHumandReadableNameExtension
{
	public static string ToHumanReadableName(this Type type)
	{
		if (type.IsGenericType)
		{
			string genericTypeName = type.GetGenericTypeDefinition().FullName!;
			int backtickIndex = genericTypeName?.IndexOf('`') ?? -1;

			if (backtickIndex > 0)
				genericTypeName = genericTypeName!.Substring(0, backtickIndex);

			IEnumerable<string> genericArguments =
				type
				.GetGenericArguments()
				.Select(arg => arg.ToHumanReadableName());

			return $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
		}

		if (type.IsArray)
		{
			Type? elementType = type.GetElementType();
			int rank = type.GetArrayRank();

			// Handle multi-dimensional arrays
			string rankSpecifier = rank == 1 ? "[]" : $"[{new string(',', rank - 1)}]";
			return $"{elementType?.ToHumanReadableName()}{rankSpecifier}";
		}

		// Handle nullable types
		if (Nullable.GetUnderlyingType(type) is Type underlyingType)
			return $"System.Nullable<{underlyingType.ToHumanReadableName()}>";

		return type.FullName ?? type.Name;
	}
}
