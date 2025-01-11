namespace Morris.AutoRegister;

/// <summary>
/// Specifies what should be used as the service key.
/// </summary>
#if PublicContracts
public
#else
internal
#endif
enum RegisterAs
{
	/// <summary>
	/// The service key will be the type specified in the filter criteria.
	/// </summary>
	SearchedType,
	/// <summary>
	/// The service key will be the type specified in the filter criteria as a closed-generic type. E.g. List&lt;int&gt; rather than List&lt;&gt;.
	/// </summary>
	SearchedTypeAsClosedGeneric,
	/// <summary>
	/// The service key will be the class discovered.
	/// </summary>
	DiscoveredClass,
	/// <summary>
	/// A service key will registered for the first interface
	/// found declared on the class discovered.
	/// </summary>
	FirstDiscoveredInterfaceOnClass
}
