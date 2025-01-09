namespace Morris.AutoInjectTests.ModuleWeaverTests.FindTests;

[TestClass]
public class DescendantsOfTests
{
	[TestMethod]
	public void WhenFindingAnOpenGenericInterface_ThenMatchesClosedGenericDescendants()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;

			[AutoInject(Find.DescendantsOf, typeof(IGeneric<>), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule {}

			public interface IGeneric<T> {}
			public interface IDescendant : IGeneric<int> {}
			public class SomeClass : IDescendant {}
			""";

		WeaverExecutor.Execute(sourceCode, out Fody.TestResult fodyTestResult, out string? manifest);
	}
}
