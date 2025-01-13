![](./Images/logo-small.png)
# AutoRegister
Reflection-free convention-based dependency registration.

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
* [Installation](./Docs/installation.md)
* [Source control](./Docs/source-control.md)
* [Examples](./Docs/examples.md)
* [Release notes](./Docs/releases.md)
