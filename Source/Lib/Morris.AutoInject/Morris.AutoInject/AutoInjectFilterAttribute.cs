#if NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Morris.AutoInject;

/// <summary>
/// Restricts the scanning of classes to only candidates that
/// match the given regex.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AutoInjectFilterAttribute : Attribute
{
	/// <summary>
	/// If not null, then only depdendency classes with a full name matching
	/// this regular expression will be registered.
	/// </summary>
#if NET9_0_OR_GREATER
[StringSyntax(StringSyntaxAttribute.Regex)]
#endif
	public string? ServiceImplementationRegex { get; set; }

	public AutoInjectFilterAttribute(
#if NET9_0_OR_GREATER
[StringSyntax(StringSyntaxAttribute.Regex)]
#endif
		string serviceImplementationRegex)
	{
		ServiceImplementationRegex = serviceImplementationRegex;
	}
}
