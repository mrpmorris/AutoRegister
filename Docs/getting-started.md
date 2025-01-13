# Getting started

* [Installation](#installation)
* [First steps](#first-steps)
* [Identifying which classes to inject](#search-criteria)
* [Specifying the service type](#specifying-the-service-type)
* [Specifying the service lifetime](#specifying-the-service-lifetime)
* [Filtering](#filtering)

<a name="installation"></a>
## Installation
To do

<a name="first-steps"></a>
## First steps
1. Create a partial class to hold your registrations.
1. Decorate it with `AutoRegister` attributes.
1. Call the static method `MyClassName.RegisterServices(IServiceCollection services)`
   from any projects that require those services.

```c#
// ConsumingProject.csproj
BusinessLayer.DependencyRegistration.RegisterServices(services);

// BusinessLayer.csproj
namespace BusinessLayer;

// AutoRegister and AutoRegisterFilter attributes go here
public partial class DependencyRegistration {}
```



<a id="search-criteria"></a>
## Identifying which classes to inject
You declare a convention using the `[AutoRegister]` attribute.


The first two arguments you pass determine which classes in the current project
should be considered candidates for injection, `Find` and `Type`. Every class
in the project (candidate) is compared against these two arguments to determine if
they should be registered or not.

`Find
* **Find.Exactly**: The candidate is valid if 
    * It is the exact type specified in `Type`,
    * or it implements the exact interface specified in `Type`.
* **Find.DescendantsOf**: The candidate is valid if
    * It is a descendant of `Type`,
    * it implements an interface that is a descendant of `Type`.
* **Find.AnyTypeOf**: The candidate is valid if
    * It is the exact type specified in `Type`,
    * it descends from `Type`, 
    * it implements the exact interface specified in `Type`,
    * or it implements an interface descended from `Type`.

`Type`
* The type you wish to scan for.
  * If you specify a class, then it is compared against the candidate,
  * if you specify an interface, then interfaces implemented by the candidate
    are compared.

<a id="specifying-the-service-type"></a>
## Specifying the service type
When a candidate matches the `Find` and `Type` criteria it will be registered. The
third argument `RegisterAs` determines how the service should be registered.

* **RegisterAs.SearchedType**:
    * The exact type you entered for the `Type` argument will be used to
      register the class.

```c#
[AutoRegister(Find.Exactly, typeof(IInterface), RegisterAs.SearchedType, WithLifetime.Scoped)]
// => AddScoped(typeof(IInterface), typeof(ClassImplementingIInterface))

[AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.SearchedType, WithLifetime.Scoped)]
// => AddScoped(typeof(BaseClass), typeof(DescendantOfBaseClass))
```

* **RegisterAs.DiscoveredClass**:
    * The candidate class's type will be used to register the class

```c#
[AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
// => AddScoped(typeof(DescendantBaseThatClass), typeof(DescendantOfBaseClass))

[AutoRegister(Find.Exactly, typeof(IScoped), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
// AddScoped(typeof(ClassImplementingIScoped), typeof(ClassImplementingIScoped))
```

* **RegisterAs.SearchedTypeAsClosedGeneric**:
    * If the `Type` is an open generic (e.g. `IRepository<,>`) then the
      generic type will be determined (e.g. `IRepository<int, string>`) and the
      service will be registered using that type.
    * If the `Type` is not an open generic then this will
      fall back to use `RegisterAs.SearchedType`.

```c#
[AutoRegister(Find.Exactly, typeof(IRepository<,>), RegisterAs.SearchedTypeAsClosedGeneric, WithLifetime.Scoped)]
// => AddScoped(typeof(IRepository<Guid, Person>), typeof(PersonRepository))

[AutoRegister(Find.DescendantsOf, typeof(Factory<>), RegisterAs.SearchedTypeAsClosedGeneric, WithLifetime.Scoped)]
// => AddScoped(typeof(Factory<Car>), typeof(CarFactory))
```

* **RegisterAs.FirstDiscoveredInterfaceOnClass**:
    * Finds the first interface on the candidate class and uses that
      as the service type
    * If the `Type` being searched for is an interface, then the first interface
      found that is *not* the same as `Type` is registered.

```c#
[AutoRegister(Find.Descendantsof, typeof(IRepository), RegisterAs.FirstDiscoveredInterfaceOnClass, WithLifetime.Scoped)]
// => AddScoped(typeof(IPersonRepository), typeof(PersonRepository))
```

**Note**: It is possible to further filter the interface to register using the
`ServiceTypeFilter` property on `[AutoInjectAttribute]`.

```c#
[AutoRegister(
    Find.Descendantsof,
    typeof(IRepository),
    RegisterAs.FirstDiscoveredInterfaceOnClass,
    WithLifetime.Scoped,
    // Regex: Service type full name must end with "Repository".
    ServiceTypeFilter = "Repository$)]
// => AddScoped(typeof(IPersonRepository), typeof(PersonRepository))
```
