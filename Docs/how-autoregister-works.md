# How it works

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
`{ProjectName}.Morris.AutoRegister.csv` that lists all the registrations. This
ensures the process is not a "black box", but instead fully auditable.
