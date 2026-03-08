![](./Images/logo-small.png)

# AutoRegister

**Reflection-free, build-time dependency registration for `Microsoft.Extensions.DependencyInjection`.**

AutoRegister lets you describe your service-registration conventions once and then generates the `IServiceCollection` registrations for you at build time.

That means:

* no runtime assembly scanning
* no reflection-heavy startup work
* no repetitive `services.AddScoped<,>()` boilerplate
* fewer missed registrations when new implementations are added

AutoRegister uses a **Roslyn source generator** plus a **Fody weaver** to turn simple attributes into fast, explicit DI registrations.

## Why use AutoRegister?

Many DI-related libraries discover implementations by scanning assemblies at startup. That works, but it also means repeated reflection and runtime work that your app has to do every time it starts.

AutoRegister moves that work to **build time** instead:

* **Faster startup** because registrations are generated before your app runs
* **Safer refactoring** because new matching classes are automatically included
* **Cleaner composition roots** because you call one generated method
* **Auditable output** because AutoRegister emits a manifest file showing exactly what was registered

If you like registration by convention, but do not want the cost and uncertainty of runtime scanning, AutoRegister is built for that.

## What it looks like

Register every implementation of `IPaymentStrategy` as a scoped `IPaymentStrategy`:

```csharp
using Morris.AutoRegister;

namespace BusinessLayer;

[AutoRegister(
	Find.Exactly,
	typeof(IPaymentStrategy),
	RegisterAs.SearchedType,
	WithLifetime.Scoped)]
public partial class DependencyRegistration
{
}
```

After building, call the generated registration method from your app startup:

```csharp
BusinessLayer.DependencyRegistration.RegisterServices(services);
```

If you later add `ApplePayPaymentStrategy`, `CardPaymentStrategy`, or `PayPalPaymentStrategy`, they are picked up automatically as long as they match the convention.

## Quick start

In the project that contains your service implementations:

1. Add the NuGet packages
2. Enable the Fody weaver
3. Create a partial registration class
4. Decorate it with one or more `[AutoRegister]` attributes
5. Call `YourModule.RegisterServices(services)` from the consuming application

### 1) Install packages

```bash
dotnet add package Fody
dotnet add package Morris.AutoRegister.Fody
```

### 2) Configure Fody

Create `FodyWeavers.xml` if it does not already exist:

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Morris.AutoRegister />
</Weavers>
```

### 3) Create a module

```csharp
using Morris.AutoRegister;

namespace BusinessLayer;

[AutoRegister(
	Find.DescendantsOf,
	typeof(BaseClass),
	RegisterAs.SearchedType,
	WithLifetime.Scoped)]
public partial class DependencyRegistration
{
}
```

### 4) Use the generated registrations

```csharp
BusinessLayer.DependencyRegistration.RegisterServices(services);
```

## How it works

AutoRegister uses a two-step pipeline:

1. **Source generator**  
   Finds classes decorated with `[AutoRegister]` or `[AutoRegisterFilter]` and generates a `RegisterServices(IServiceCollection services)` method so your consuming project can compile and call it immediately.

2. **Fody weaver**  
   After the project is built, AutoRegister scans the compiled assembly, finds matching types, replaces the stubbed method body with real `AddScoped`, `AddSingleton`, or `AddTransient` calls, and writes a registration manifest beside your project file.

The result is close to handwritten registrations, without the maintenance burden.

## Registration options

### Search criteria: `Find`

Choose which types should be discovered:

* `Find.Exactly` - match the exact type, or classes implementing the exact interface
* `Find.DescendantsOf` - match descendants of a class or descendants of an interface
* `Find.AnyTypeOf` - match the exact type plus descendants

### Service key: `RegisterAs`

Choose how the discovered type should be registered:

* `RegisterAs.SearchedType`
* `RegisterAs.SearchedTypeAsClosedGeneric`
* `RegisterAs.ImplementingClass`
* `RegisterAs.FirstDiscoveredInterfaceOnClass`
* `RegisterAs.DiscoveredType`

### Lifetime: `WithLifetime`

Choose the DI lifetime:

* `WithLifetime.Singleton`
* `WithLifetime.Scoped`
* `WithLifetime.Transient`

### Filtering

You can narrow registration with regex-based filters:

* `[AutoRegisterFilter(...)]` filters the classes considered by the module
* `ServiceImplementationTypeFilter` filters candidate implementation types
* `ServiceTypeFilter` filters the service types used during registration

Example:

```csharp
[AutoRegisterFilter(@"^(?!.*\.OptionalPlugins\.).*$")]
[AutoRegister(
	Find.AnyTypeOf,
	typeof(IValidator<>),
	RegisterAs.SearchedTypeAsClosedGeneric,
	WithLifetime.Scoped,
	ServiceImplementationTypeFilter = @"\.MandatoryValidations\.")]
public partial class DependencyRegistration
{
}
```

## Examples

### Register all implementations of an interface

```csharp
[AutoRegister(
	Find.Exactly,
	typeof(IPaymentStrategy),
	RegisterAs.SearchedType,
	WithLifetime.Scoped)]
public partial class DependencyRegistration
{
}
```

### Register open generic descendants as closed generic services

```csharp
[AutoRegister(
	Find.DescendantsOf,
	typeof(Repository<>),
	RegisterAs.SearchedTypeAsClosedGeneric,
	WithLifetime.Scoped)]
public partial class DependencyRegistration
{
}
```

### Register each class as its first discovered interface

```csharp
[AutoRegister(
	Find.DescendantsOf,
	typeof(RepositoryBase<>),
	RegisterAs.FirstDiscoveredInterfaceOnClass,
	WithLifetime.Scoped)]
public partial class DependencyRegistration
{
}
```

### Add manual registrations too

You can extend the generated method by implementing:

```csharp
static partial void AfterRegisterServices(IServiceCollection services)
{
	// Add any registrations that should happen after convention-based registration
}
```

## Auditable output

Every build generates a manifest file named:

```text
{ProjectName}.Morris.AutoRegister.csv
```

The manifest lists:

* module
* attribute
* lifetime
* service type
* implementation type

This makes AutoRegister easier to review in pull requests and safer to use in larger teams because the generated registrations are not a black box.

## Build and test this repository

```bash
dotnet build "./Source/" --configuration Release
dotnet test "./Source/" --no-build --configuration Release
```

## Documentation

* [Benefits of registration by convention](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/benefits-of-registration-by-convention.md)
* [How AutoRegister works](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/how-autoregister-works.md)
* [Getting started](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/getting-started.md)
* [Source control and manifest files](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/source-control.md)
* [Examples](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/examples.md)
* [Release notes](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/releases.md)
