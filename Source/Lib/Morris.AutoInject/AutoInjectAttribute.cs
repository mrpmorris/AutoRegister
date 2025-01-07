#if NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

using System;

namespace Morris.AutoInject;

/// <summary>
/// Scans the assembly and registers all dependencies that match
/// the given criteria.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
#if PublicContracts
public
#else
internal
#endif
class AutoInjectAttribute : Attribute
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
	public string? ServiceIdentifierFilter { get; set; }

	/// <summary>
	/// If not null, then only depdendency classes with a full name matching
	/// this regular expression will be registered.
	/// </summary>
#if NET9_0_OR_GREATER
	[StringSyntax(StringSyntaxAttribute.Regex)]
#endif
	public string? ServiceImplementationFilter { get; set; }

	/// <summary>
	/// The type to use when scanning for candidates to register.
	/// </summary>
	public Type Type { get; set; }

	/// <summary>
	/// The lifetime to use when registering the dependency.
	/// </summary>
	public WithLifetime WithLifetime { get; set; }

	/// <summary>
	/// Creates a new instance of the attribute.
	/// </summary>
	/// <param name="find"><see cref="AutoInjectAttribute.Find"/></param>
	/// <param name="type"><see cref="AutoInjectAttribute.Type"/></param>
	/// <param name="registerAs"><see cref="AutoInjectAttribute.RegisterAs"/></param>
	/// <param name="withLifetime"><see cref="AutoInjectAttribute.WithLifetime"/></param>
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
