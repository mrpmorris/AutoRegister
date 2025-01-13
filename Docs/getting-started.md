# Getting started

* [Installation](#installation)
* [First steps](#first-steps)
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



<a id="search-criteria"></a>
## Identifying which classes to inject
You declare a convention using the `[AutoRegister]` attribute.

<a id="specifying-the-service-key"></a>
## Specifying the service key
To do

