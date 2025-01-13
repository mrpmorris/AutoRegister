using Mono.Cecil;
using Morris.AutoRegister.Fody.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Morris.AutoRegister.Fody;
using ServiceTypeAndImplementation = (TypeReference ServiceType, TypeReference ServiceImplementation);

internal class AutoRegisterAttributeData
{
	public Find Find { get; private set; }
	public RegisterAs Register { get; private set; }
	public string? ServiceTypeFilter { get; private set; }
	public string? ServiceImplementationTypeFilter { get; private set; }
	public TypeDefinition Type { get; private set; }
	public WithLifetime WithLifetime { get; private set; }

	private readonly Func<TypeReference, TypeReference?> GetKey;
	private readonly Func<TypeReference, IEnumerable<TypeReference>> GetPotentialKeys;
	private readonly Regex? ServiceImplementationTypeFilterRegex;
	private readonly Regex? ServiceTypeFilterRegex;
	private readonly Func<ServiceTypeAndImplementation, TypeReference?> TransformKey;

	public AutoRegisterAttributeData(
		Find find,
		RegisterAs registerAs,
		string? serviceTypeFilter,
		string? serviceImplementationTypeFilter,
		TypeReference type,
		WithLifetime withLifetime)
	{
		Find = find;
		Register = registerAs;
		ServiceTypeFilter = serviceTypeFilter;
		ServiceImplementationTypeFilter = serviceImplementationTypeFilter;
		Type = type.Resolve();
		WithLifetime = withLifetime;

		if (serviceTypeFilter is not null)
			ServiceTypeFilterRegex = new Regex(serviceTypeFilter, RegexOptions.Compiled);

		if (serviceImplementationTypeFilter is not null)
			ServiceImplementationTypeFilterRegex = new Regex(serviceImplementationTypeFilter, RegexOptions.Compiled);

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
			RegisterAs.SearchedType => _ => Type,
			RegisterAs.SearchedTypeAsClosedGeneric => x => x.ServiceType.GetBaseClosedGenericType(Type),
			RegisterAs.FirstDiscoveredInterfaceOnClass => x => GetFirstInterface(x.ServiceImplementation),
			_ => throw new NotImplementedException(Register.ToString())
		};
	}

	public bool IsMatch(
		TypeReference type,
		out TypeReference? serviceType)
	{
		bool serviceImplementationMatch =
			ServiceImplementationTypeFilterRegex is null
			|| ServiceImplementationTypeFilterRegex.IsMatch(type.ToHumanReadableName());

		if (!serviceImplementationMatch)
		{
			serviceType = null;
			return false;
		}

		serviceType =
			GetPotentialKeys(type)
			.Select(GetKey)
			.FirstOrDefault();

		if (serviceType is not null)
			serviceType = TransformKey((serviceType, type));
		bool serviceTypeMatch = DoesServiceTypeMatch(serviceType);

		return serviceType is not null && serviceTypeMatch;
	}

	private bool DoesServiceTypeMatch(TypeReference? serviceType) =>
		ServiceTypeFilterRegex is null
		||
		(
			serviceType is not null
			&& ServiceTypeFilterRegex.IsMatch(serviceType.ToHumanReadableName())
		);

	private TypeReference? FindAnyTypeOf(TypeReference typeReference) =>
		typeReference.Resolve().IsAssignableTo(Type)
		? typeReference
		: null;

	private TypeReference? FindDescendantsOf(TypeReference typeReference) =>
		typeReference.Resolve().DescendsFrom(Type)
		? typeReference
		: null;

	private TypeReference? FindExactly(TypeReference typeReference) =>
		typeReference.IsSameAs(Type)
		? typeReference
		: null;

	private TypeReference? GetFirstInterface(TypeReference typeReference) =>
		typeReference
		.Resolve()
		.GetAllInterfaces()
		.Where(DoesServiceTypeMatch)
		.FirstOrDefault();
}
