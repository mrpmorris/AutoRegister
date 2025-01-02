namespace Morris.AutoInject;

/// <summary>
/// Specifies the search criteria.
/// </summary>
public enum Find
{
	/// <summary>
	/// Only considers descendants of the specified type
	/// as candidates for registration.
	/// </summary>
	DescendantsOf,
	/// <summary>
	/// Considers descendants of the specified type
	/// and the specified type itself as candidates for registration.
	/// </summary>
	AnyTypeOf,
	/// <summary>
	/// Only considers the exact specified type when determining
	/// candidates for registration.
	/// </summary>
	Exactly
}
