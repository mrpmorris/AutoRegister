using System.Collections.Generic;

namespace Morris.AutoRegister.Fody.Extensions;

internal static class IEnumerableAppendExtension
{
	public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
	{
		foreach(T sourceItem in source)
			yield return sourceItem;
		yield return item;
	}
}
