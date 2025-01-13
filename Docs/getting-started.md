# Getting started

## Next steps

* [Installation](#installation)
* [First steps](#first-steps)
* [How it works](#how-it-works)
* [Benefits of registration by convention](#benefits-of-registration-by-convention)
* [Identifying which classes to inject](#search-criteria)
* [Specifying the service key](#specifying-the-service-key)
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

<a id="how-it-works"></a>
## How it works
First, I have a Roslyn source generator that looks for classes decorated with either
the `[AutoRegister]` attribute or the `[AutoRegisterFilter]` attribute. When it finds
such a class, it generates the following source code. This is to ensure you are able
to call `RegisterServices` from the consuming project.

```c#
using Microsoft.Extensions.DependencyInjection;

namespace YourNamespace
{
  partial class YourClassName
  {
    static partial void AfterRegisterServices(IServiceCollection services);
    public static void RegisterServices(IServiceCollection services)
    {
    }
  }
}
```

Because Roslyn regenerates source on every keypress, scanning all classes in the
project is not recommended. So I do that after the project is built instead.

I then process each `[AutoFilter]` attribute and generate the required registration
code in the `RegisterServices` method. In addition, I output a
[manifest](./source-control.md) file with the name
`{ProjectName}.Morris.AutoRegister.manifest` that lists all the registrations. This
ensures the process is not a "black box", but instead fully auditable.

<a id="benefits-of-registration-by-convention"></a>
## Benefits of registration by convention
Imagine you are asked to write a new service that implements `IPaymentStrategy`. After
completing your code, you must also remember to add the following line of code somewhere

```c#
services.AddScoped<IPaymentStrategy, MyNewPaymentStrategy>();
```

It's not a mega pain to do, but how many times have you forgotten to do it?

Perhaps you use a library such as [MediatR](https://github.com/jbogard/MediatR) that has to
scan your assembly at app-startup for classes implementing `IRequestHandler<TRequest, TResponse>`?

```c#
services.AddMediatR(x =>
   x.RegisterServicesFromAssembly(typeof(SomeType).Assembly)
);
```

Perhaps you also use [Fluent Validation](https://github.com/FluentValidation/FluentValidation) in
your app, which then does its own scan of assemblies to register classes?

**Reflection is slow and multiple assembly scans are bad!**

Ideally, we want to know everything to register at compile-time so we can avoid
reflection and assembly scanning altogether, but writing the registration code
manually is a pain, and error prone.

**AutoRegister** releaves you of your pain. Registration by convention means you
now write code like this:

```c#
[AutoRegister(
   Find.Exactly,
   typeof(IPaymentStrategy), // Find classes implementing this interface
   RegisterAs.SearchedType, // Service key is IPaymentStrategy
   WithLifetime.Scoped // Register discovered classes as Scoped services
)]
public partial class DependencyRegistration {}
```

When you add a new class that implements `IPaymentStrategy` it will automatically
be registered in the generated `DependencyRegistration.RegisterServices(IServiceCollection services)`
method.

*Generated code*
```c#
partial class DependencyRegistration
{
  public static void RegisterServices(IServiceCollection services)
  {
    services.AddScoped(typeof(IPaymentService), typeof(ExistingPaymentService1));
    services.AddScoped(typeof(IPaymentService), typeof(ExistingPaymentService2));
    services.AddScoped(typeof(IPaymentService), typeof(MyNewPaymentService));
  }
}
```
* No assembly scanning!
* No reflection!
* Faster app start-ups!
* As fast as if you'd written it manually, but without the effort!

For accountability, AutoReflect even generates a
[manifest file](./source-control.md) you can check into source control, to
see which classes are being registered.

<a id="search-criteria"></a>
## Identifying which classes to inject
You declare a convention using the `[AutoRegister]` attribute.

<a id="specifying-the-service-key"></a>
## Specifying the service key
To do

