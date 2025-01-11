namespace Morris.AutoRegisterTests.Extensions;

internal static class TypeToHumandReadableNameExtension
{
	public static string ToHumanReadableName(this Type type)
	{
		if (Nullable.GetUnderlyingType(type) is Type underlyingType)
			return $"System.Nullable<{underlyingType.ToHumanReadableName()}>";

		string name;
		if (type.IsGenericType)
		{
			string genericTypeName = type.GetGenericTypeDefinition().Name;
			int backtickIndex = genericTypeName?.IndexOf('`') ?? -1;

			if (backtickIndex > 0)
				genericTypeName = genericTypeName!.Substring(0, backtickIndex);

			IEnumerable<string> genericArguments =
				type
				.GetGenericArguments()
				.Select(arg => arg.ToHumanReadableName());

			name = $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
		}
		else if (type.IsArray)
		{
			Type? elementType = type.GetElementType();
			int rank = type.GetArrayRank();

			// Handle multi-dimensional arrays
			string rankSpecifier = rank == 1 ? "[]" : $"[{new string(',', rank - 1)}]";
			name = $"{elementType!.ToHumanReadableName()}{rankSpecifier}";
		}
		else
			name = type.Name;

		if (type.DeclaringType is not null)
			return $"{type.DeclaringType.ToHumanReadableName()}+{name}";

		if (type.Namespace is null)
			return name;

		return $"{type.Namespace}.{name}";
	}
}
