#if NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

using System;

namespace Morris.AutoRegister;

/// <summary>
/// Restricts the scanning of classes to only candidates that
/// match the given regex.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
#if PublicContracts
public
#else
internal
#endif
class AutoRegisterFilterAttribute : Attribute
{
	/// <summary>
	/// If not null, then only depdendency classes with a full name matching
	/// this regular expression will be registered.
	/// </summary>
#if NET9_0_OR_GREATER
	[StringSyntax(StringSyntaxAttribute.Regex)]
#endif
	public string? ServiceImplementationFilter { get; set; }

	/// <summary>
	/// Creates a new instance of the attribute.
	/// </summary>
	/// <param name="serviceImplementationFilter"><see cref="AutoRegisterFilterAttribute.ServiceImplementationFilter"/></param>
	public AutoRegisterFilterAttribute(
#if NET9_0_OR_GREATER
		[StringSyntax(StringSyntaxAttribute.Regex)]
#endif
		string serviceImplementationFilter)
	{
		ServiceImplementationFilter = serviceImplementationFilter;
	}
}
