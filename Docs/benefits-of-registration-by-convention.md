# Benefits of registration by convention

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
