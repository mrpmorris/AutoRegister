﻿#nullable enable
#if NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Morris.AutoInject;

/// <summary>
/// Scans the assembly and registers all dependencies that match
/// the given criteria.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AutoInjectAttribute : Attribute
{
	/// <summary>
	/// The criteria to use when scanning for candidates to register.
	/// </summary>
	public Find Find { get; set; }

	/// <summary>
	/// Specifies what should be used as the service key when
	/// registering the dependency.
	/// </summary>
	public RegisterAs RegisterAs { get; set; }

	/// <summary>
	/// If not null, then only service key types with a full name matching
	/// this regular expression will be registered.
	/// </summary>
#if NET9_0_OR_GREATER
[StringSyntax(StringSyntaxAttribute.Regex)]
#endif
	public string? ServiceKeyRegex { get; set; }

	/// <summary>
	/// If not null, then only depdendency classes with a full name matching
	/// this regular expression will be registered.
	/// </summary>
#if NET9_0_OR_GREATER
[StringSyntax(StringSyntaxAttribute.Regex)]
#endif
	public string? ServiceImplementationRegex { get; set; }

	/// <summary>
	/// The type to use when scanning for candidates to register.
	/// </summary>
	public Type Type { get; set; }

	/// <summary>
	/// The lifetime to use when registering the dependency.
	/// </summary>
	public WithLifetime WithLifetime { get; set; }

	public AutoInjectAttribute(
		Find find,
		Type type,
		RegisterAs registerAs,
		WithLifetime withLifetime)
	{
		Find = find;
		Type = type;
		RegisterAs = registerAs;
		WithLifetime = withLifetime;
	}
}





/// <summary>
/// Specifies the lifetime of a service.
/// </summary>

#nullable restore