# Examples

* [Register a single class](#register-a-single-class)
* [Register classes implementing a marker interface](#register-classes-with-a-marker-interface) 
* [Register classes descended from RepositoryBase<,>](#register-classes-descneded-from-repositorybase)
* [Register all IPaymentStrategy implementations](#register-all-ipaymentstrategy-implementations)
* [Register interfaces descended from IRepository](#register-interfaces-descended-from-irepository)
* [Register first interface matching name `*IRepository`](#register-first-interface-name-matching-repository)


<a id="register-a-single-class"></a>
## Register a single class
This code will register `MyService` as Scoped.
```c#
[AutoRegister(
   Find.Exactly,
   typeof(MyService),
   RegisterAs.ImplementingClass,
   WithLifetime.Scoped)]
// => services.AddScoped(typeof(MyService), typeof(MyService))
public partial class DependencyRegistration {}
```


Alternatively, you can implement the following method on your registration
class. Any registrations made in this method will **NOT** appear in your
[manifest file](./source-control.md).

```c#
static partial void AfterRegisterServices(IServiceCollection services)
{
   // This is called at the end of the RegisterServices method
}
```

<a id="register-classes-with-a-marker-interface"></a>
## Register classes implementing a marker interface
This code will find all classes that implement either 
fictitious `IScoped` or `ISingleton` and register the class.

```c#
[AutoRegister(
   Find.Exactly,
   typeof(IScoped),
   RegisterAs.ImplementingClass,
   WithLifetime.Scoped)]
// => services.AddScoped(typeof(ScopedService1), typeof(ScopedService1))
// => services.AddScoped(typeof(ScopedService2), typeof(ScopedService2))
[AutoRegister(
   Find.Exactly,
   typeof(ISingleton),
   RegisterAs.ImplementingClass,
   WithLifetime.Singleton)]
// => services.AddScoped(typeof(SingletonService1), typeof(SingletonService1))
// => services.AddScoped(typeof(SingletonService2), typeof(SingletonService2))
public partial class DependencyRegistration {}
```

<a id="register-classes-descneded-from-repositorybase"></a>
## Register classes descended from RepositoryBase<,>
This code will find all classes that descend from RepositoryBase&lt;,&gt;.
The service key will be `RepositoryBase<,>` with the generic
parameters filled in.

```c#
[AutoRegister(
   Find.DescendantsOf,
   typeof(RepositoryBase<,>),
   RegisterAs.SearchedTypeAsClosedGeneric,
   WithLifetime.Scoped)]
public partial class DependencyRegistration {}

// => services.AddScoped(typeof(EmployeeRepository<PersonId, Person>), typeof(EmployeeRepository))
// => services.AddScoped(typeof(CompanyRepository<CompanyId, Company>), typeof(CompanyRepository))
```

<a id="register-all-ipaymentstrategy-implementations"></a>
## Register all IPaymentStrategy implementations
This code will find all classes that implement `IPaymentStrategy` and register
the class using `IPaymentStrategy` as the injectable service type.

```c#
[AutoRegister(
   Find.Exactly,
   typeof(IPaymentStrategy),
   RegisterAs.SearchedType,
   WithLifetime.Scoped)]
// => services.AddScoped(typeof(IPaymentStrategy), typeof(PaymentStrategy1))
// => services.AddScoped(typeof(IPaymentStrategy), typeof(PaymentStrategy2))
public partial class DependencyRegistration {}
```


<a id="register-interfaces-descended-from-irepository"></a>
## Register interfaces descended from IRepository
```c#
[AutoRegister(
   Find.AnyTypeOf,
   typeof(IRepository),
   RegisterAs.DiscoveredType,
   WithLifetime.Scoped)]
// => services.AddScoped(typeof(ICustomerRepository), typeof(CustomerRepository))
// => services.AddScoped(typeof(IOrderRepository), typeof(OrderRepository))
```

<a id="register-first-interface-name-matching-repository"></a>
## Register interfaces descended from IRepository
This code will find all classes that implement an interface
that descends from `IRepository` and register them as the
first interface on the candidate class that has a full name
ending with "Repository".

```c#
[AutoRegister(
   Find.DescendantsOf,
   typeof(IRepository),
   RegisterAs.FirstDiscoveredInterfaceOnClass,
   WithLifetime.Scoped,
   ServiceTypeFilter = "Repository$)]
// => services.AddScoped(typeof(IPersonRepository), typeof(PersonRepository))
public partial class DependencyRegistration {}
```
