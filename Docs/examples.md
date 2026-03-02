# Examples

* [Register all IPaymentStrategy implementations as IPaymentStrategy](#register-all-ipaymentstrategy-implementations)
* [Register classes descended from Repository&lt;&gt; as Repository&lt;Person&gt; etc](#register-classes-descended-from-repository)
* [Register classes descended from RepositoryBase&lt;&gt; as IPersonRepository etc](#register-classes-descended-from-repositorybase-as-first-discovered-interface)
* [Register classes implementing interfaces descended from IRepository (IPersonRepository etc)](#register-interfaces-descended-from-irepository)
* [Register first interface matching name `*Repository`](#register-first-interface-name-matching-repository)
* [Register a single class](#register-a-single-class)
* [Register classes implementing a marker interface](#register-classes-with-a-marker-interface) 


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


<a id="register-classes-descended-from-repository"></a>
## Register classes descended from Repository&lt;&gt; as Repository&lt;Person&gt; etc.
This code will find all classes that descend from Repository&lt;&gt;.
The service key will be `Repository<>` with the generic
parameters filled in (`Repository<Person>`, `Repository<Company>`, etc).

```c#
[AutoRegister(
   Find.DescendantsOf,
   typeof(Repository<>),
   RegisterAs.SearchedTypeAsClosedGeneric,
   WithLifetime.Scoped)]
public partial class DependencyRegistration {}

// => services.AddScoped(typeof(Repository<Person>), typeof(EmployeeRepository))
// => services.AddScoped(typeof(Repository<Company>), typeof(CompanyRepository))
```


<a id="register-classes-descended-from-repositorybase-as-first-discovered-interface"></a>
## Register classes descended from RepositoryBase&lt;&gt; as IPersonRepository etc.
This code will find all classes that descend from RepositoryBase&lt;&gt;.
The service key will be the first interface discovered on the class.

```c#
[AutoRegister(
   Find.DescendantsOf,
   typeof(RepositoryBase<>),
   RegisterAs.FirstDiscoveredInterfaceOnClass,
   WithLifetime.Scoped)]
public partial class DependencyRegistration {}

// => services.AddScoped(typeof(IPersonRepository), typeof(EmployeeRepository))
// => services.AddScoped(typeof(ICompanyRepository), typeof(CompanyRepository))
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
