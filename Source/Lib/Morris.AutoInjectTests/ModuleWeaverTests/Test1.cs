using Morris.AutoInject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class Test1
{
	[TestMethod]
	public void X() { }
}

[AutoInject(Find.DescendantsOf, typeof(System.Object), RegisterAs.FirstDiscoveredInterface, WithLifetime.Scoped)]
public partial class MyModule
{

}
