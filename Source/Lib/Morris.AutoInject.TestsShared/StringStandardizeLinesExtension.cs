using System.Text.RegularExpressions;

namespace Morris.AutoInject.TestsShared;

public static class StringStandardizeLinesExtension
{
	public static string? StandardizeLines(this string? value) =>
		value is null
		? null
		: Regex
			.Replace(
				input: value.Replace("\r", ""),
				pattern: "(?m)^\t+",
				evaluator: x => new string(' ', x.Length * 4)
			)
			.Trim();
}
