using Mono.Cecil;
using Morris.AutoInject.Fody.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morris.AutoInject.Fody;
using ServiceTypeAndImplementation = (TypeDefinition ServiceType, TypeDefinition ServiceImplementation);

internal class AutoInjectAttributeData
{
	public Find Find { get; private set; }
	public RegisterAs Register { get; private set; }
	public string? ServiceTypeFilter { get; private set; }
	public string? ServiceImplementationFilter { get; private set; }
	public TypeDefinition Type { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	private readonly Func<TypeDefinition, TypeDefinition?> GetKey;
	private readonly Func<TypeDefinition, IEnumerable<TypeDefinition>> GetPotentialKeys;
	private readonly Func<ServiceTypeAndImplementation, TypeDefinition> TransformKey;

	public AutoInjectAttributeData(
		Find find,
		RegisterAs registerAs,
		string? serviceTypeFilter,
		string? serviceImplementationFilter,
		TypeReference type,
		WithLifetime withLifetime)
	{
		Find = find;
		Register = registerAs;
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

		TransformKey = Register switch {
			RegisterAs.DiscoveredClass => x => x.ServiceImplementation,
			RegisterAs.BaseType => _ => Type,
			_ => throw new NotImplementedException(Register.ToString())
		};
	}

	public bool IsMatch(
		TypeDefinition type,
		out TypeDefinition? serviceType)
	{
		serviceType =
			GetPotentialKeys(type)
			.Select(GetKey)
			.FirstOrDefault();
		if (serviceType is not null)
			serviceType = TransformKey((serviceType.Resolve(), type));
		return serviceType is not null;
	}

	private TypeDefinition? FindAnyTypeOf(TypeDefinition definition) =>
		definition.IsAssignableTo(Type)
		? definition
		: null;

	private TypeDefinition? FindDescendantsOf(TypeDefinition definition) =>
		definition.DescendsFrom(Type)
		? definition
		: null;

	private TypeDefinition? FindExactly(TypeDefinition definition) =>
		definition == Type
		? definition
		: null;




}
