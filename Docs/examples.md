# Examples

* [Getting started](#getting-started)
* [Register a single class](#register-a-single-class)


<a id="getting-started"></a>
## Getting started
1. [Install](./installation.md) AutoRegister.
1. Create a partial class to hold your registrations.
1. Decorate it with `AutoRegister` attributes.
1. Call the static method `MyClassName.RegisterServices(IServiceCollection services)`
   from any projects that require those services.

```c#
// ConsumingProject.csproj
BusinessLayer.DependencyRegistration.RegisterServices(services);

// BusinessLayer.csproj
namespace BusinessLayer;

[AutoRegister(Find.Exactly, typeof(IPaymentStrategy), RegisterAs.SearchedType, WithLifetime.Scoped)]
public partial class DependencyRegistration {}
```

<a id="register-a-single-class"></a>
## Register a single class
This code will register `MyService` as Scoped.
```c#
[AutoRegister(Find.Exactly, typeof(MyService), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
public partial class DependencyRegistration {}
```

Alternatively, you can implement the following method on your registration
class.

```c#
static partial void AfterRegisterServices(IServiceCollection services)
{
   // This is called at the end of the RegisterServices method
}
```

Any registrations made here will **NOT** appear in your
[manifest file](./source-control.md).