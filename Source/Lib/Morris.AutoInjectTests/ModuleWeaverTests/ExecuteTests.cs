using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

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
			[AutoInject(Find.DescendantsOf, typeof(object), RegisterAs.FirstDiscoveredInterfaceOnClass, WithLifetime.Scoped)]
			public partial class MyModule
			{

			}
			""";

		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);

		RegistrationHelper.AssertRegistration(
			assembly: fodyTestResult.Assembly,
			manifest: manifest,
			expectedModuleRegistrations:
			[
				new(
					classFullName: "MyNamespace.MyModule",
					autoInjectAttributes: [
						new(
							find: Find.DescendantsOf,
							typeFullName: "System.Object",
							registerAs: RegisterAs.FirstDiscoveredInterfaceOnClass,
							withLifetime: WithLifetime.Scoped)
					],
					services: [
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.MyModule",
							serviceImplementationTypeFullName: "MyNamespace.MyModule"
						),
					]
				)
			]
		);
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

		RegistrationHelper.AssertRegistration(
			assembly: fodyTestResult.Assembly,
			manifest: manifest,
			expectedModuleRegistrations:
			[
				new(
					classFullName: "MyNamespace1.MyModule",
					autoInjectAttributes: [],
					services: []
				),
				new(
					classFullName: "MyNamespace2.MyModule",
					autoInjectAttributes: [],
					services: []
				),
			]);
	}
}
