namespace Morris.AutoInject;

/// <summary>
/// Specifies what should be used as the service key.
/// </summary>
public enum RegisterAs
{
	/// <summary>
	/// The service key will be the type specified in the filter criteria.
	/// </summary>
	BaseType,
	/// <summary>
	/// The service key will be the base type as a closed-generic type.
	/// </summary>
	BaseClosedGenericType,
	/// <summary>
	/// The service key will be the class discovered.
	/// </summary>
	DiscoveredClasses,
	/// <summary>
	/// A service key will registered for the first interface
	/// found declared on the class discovered.
	/// </summary>
	FirstDiscoveredInterface
}
