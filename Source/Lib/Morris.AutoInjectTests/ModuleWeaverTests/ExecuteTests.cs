using Morris.AutoInject.Fody;
using Morris.AutoInject.TestsShared;
using Morris.AutoInjectTests.Extensions;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class ExecuteTests
{
	[TestMethod]
	public void WhenClassHasAutoInjectAttribute_ThenClassAppearsInManifest()
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

		string expectedManifest =
			$$"""
			{{ModuleWeaver.ManifestHeader}}
			MyNamespace.MyModule
			,Find DescendantsOf System.Object RegisterAs FirstDiscoveredInterface
			,,Scoped,<Module>,<Module>
			,,Scoped,MyNamespace.MyModule,MyNamespace.MyModule
			""";
		Assert.AreEqual(expectedManifest.StandardizeLines(), manifest.StandardizeLines());
	}

	[TestMethod]
	public void WhenClassHasAutoInjectFilterAttribute_ThenClassAppearsInManifest()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace1
			{
				[AutoInjectFilter("SomeFilter")]
				public partial class MyModule
				{

				}
			}

			namespace MyNamespace2
			{
				[AutoInjectFilter("SomeFilter")]
				public partial class MyModule
				{
			
				}
			}
			
			""";
		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);

		string expectedManifest =
			$$"""
			{{ModuleWeaver.ManifestHeader}}
			MyNamespace1.MyModule
			MyNamespace2.MyModule
			""";
		Assert.AreEqual(expectedManifest.StandardizeLines(), manifest.StandardizeLines());
	}
}
