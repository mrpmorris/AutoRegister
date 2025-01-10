using Mono.Cecil;
using Morris.AutoInject.Fody.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morris.AutoInject.Fody;
using ServiceTypeAndImplementation = (TypeReference ServiceType, TypeReference ServiceImplementation);

internal class AutoInjectAttributeData
{
	public Find Find { get; private set; }
	public RegisterAs Register { get; private set; }
	public string? ServiceTypeFilter { get; private set; }
	public string? ServiceImplementationFilter { get; private set; }
	public TypeDefinition Type { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	private readonly Func<TypeReference, TypeReference?> GetKey;
	private readonly Func<TypeReference, IEnumerable<TypeReference>> GetPotentialKeys;
	private readonly Func<ServiceTypeAndImplementation, TypeReference?> TransformKey;

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
			RegisterAs.BaseClosedGenericType => x => x.ServiceType.GetBaseClosedGenericType(Type),
			_ => throw new NotImplementedException(Register.ToString())
		};
	}

	public bool IsMatch(
		TypeReference type,
		out TypeReference? serviceType)
	{
		serviceType =
			GetPotentialKeys(type)
			.Select(GetKey)
			.FirstOrDefault();
		if (serviceType is not null)
			serviceType = TransformKey((serviceType, type));
		return serviceType is not null;
	}

	private TypeReference? FindAnyTypeOf(TypeReference typeReference) =>
		typeReference.Resolve().IsAssignableTo(Type)
		? typeReference
		: null;

	private TypeReference? FindDescendantsOf(TypeReference typeReference) =>
		typeReference.Resolve().DescendsFrom(Type)
		? typeReference
		: null;

	private TypeReference? FindExactly(TypeReference typeReference) =>
		typeReference == Type
		? typeReference
		: null;
}
