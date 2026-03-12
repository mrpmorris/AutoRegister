# AutoRegister
![](./Images/logo-small.png)

[![NuGet version](https://img.shields.io/nuget/v/Morris.AutoRegister.Fody.svg)](https://www.nuget.org/packages/Morris.AutoRegister.Fody)
[![NuGet downloads](https://img.shields.io/nuget/dt/Morris.AutoRegister.Fody.svg)](https://www.nuget.org/packages/Morris.AutoRegister.Fody)

Reflection-free, build-time, convention-based dependency registration.

AutoRegister is a [Fody](https://github.com/Fody/Fody) plugin that scans your assembly at build time and
generates code to register your injectable dependencies.

## Example
You can register all descendants of `BaseClass` like so:
```x#
[AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.SearchedType, WithLifetime.Scoped)]
public partial class MyModule
{
}
```

# Documentation
* [Benefits of registration by convention](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/benefits-of-registration-by-convention.md)
* [How AutoRegister works](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/how-autoregister-works.md)
* [Getting started](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/getting-started.md)
* [Source control](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/source-control.md)
* [Examples](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/examples.md)
* [Release notes](https://github.com/mrpmorris/AutoRegister/blob/master/Docs/releases.md)
