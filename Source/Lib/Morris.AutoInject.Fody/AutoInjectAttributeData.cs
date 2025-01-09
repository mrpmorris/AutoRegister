using Mono.Cecil;
using Morris.AutoInject.Fody.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morris.AutoInject.Fody;

internal class AutoInjectAttributeData
{
	public Find Find { get; private set; }
	public RegisterAs RegisterAs { get; private set; }
	public string? ServiceTypeFilter { get; private set; }
	public string? ServiceImplementationFilter { get; private set; }
	public TypeDefinition Type { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	private readonly Func<TypeDefinition, TypeReference?> GetKey;
	private readonly Func<TypeDefinition, IEnumerable<TypeDefinition>> GetPotentialKeys;

	public AutoInjectAttributeData(
		Find find,
		RegisterAs registerAs,
		string? serviceTypeFilter,
		string? serviceImplementationFilter,
		TypeReference type,
		WithLifetime withLifetime)
	{
		Find = find;
		RegisterAs = registerAs;
		ServiceTypeFilter = serviceTypeFilter;
		ServiceImplementationFilter = serviceImplementationFilter;
		Type = type.Resolve();
		WithLifetime = withLifetime;

		TypeDefinition resolvedType = type.Resolve();
		GetPotentialKeys =
			Type.IsInterface
			? x => x.GetAllInterfaces()
			: x => [x];

		GetKey = Find switch {
			Find.Exactly => FindExactly,
			Find.AnyTypeOf => FindAnyTypeOf,
			Find.DescendantsOf => FindDescendantsOf,
			_ => throw new NotImplementedException(Find.ToString())
		};
	}

	public bool IsMatch(
		TypeDefinition type,
		out TypeReference? serviceType)
	{
		serviceType =
			GetPotentialKeys(type)
			.Select(GetKey)
			.FirstOrDefault();
		if (serviceType is not null && RegisterAs == RegisterAs.DiscoveredClass)
			serviceType = type;
		return serviceType is not null;
	}

	private TypeReference? FindAnyTypeOf(TypeDefinition definition) =>
		definition.IsAssignableTo(Type)
		? definition
		: null;

	private TypeReference? FindDescendantsOf(TypeDefinition definition) =>
		definition.DescendsFrom(Type)
		? definition
		: null;

	private TypeReference? FindExactly(TypeDefinition definition) =>
		definition == Type
		? definition
		: null;




}
