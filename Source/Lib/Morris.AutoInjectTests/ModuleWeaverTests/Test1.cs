using Morris.AutoInjectTests.Extensions;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class Test1
{
	[TestMethod]
	public void X()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.DescendantsOf, typeof(object), RegisterAs.FirstDiscoveredInterface, WithLifetime.Scoped)]
			public partial class MyModule
			{

			}
			""";
		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);
		fodyTestResult.AssertNoDiagnostics();
		Assert.AreEqual("MyNamespace.MyModule\n\n", manifest);
	}
}
