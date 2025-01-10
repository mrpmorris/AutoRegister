using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class FilterTests
{
	[TestMethod]
	public void WhenModuleHasAnAutoInjectFilterAttribute_ThenOnlyCandidatesMatchingTheFilterAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInjectFilter(@"\.ValidClass\d+")]
			[AutoInject(Find.AnyTypeOf, typeof(BaseClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public class BaseClass {}
			public class ValidClass1 : BaseClass {}
			public class ValidClass2 : ValidClass1 {}
			public class InvalidClass1 : BaseClass {}
			public class InvalidClass2 : InvalidClass1 {}
			""";

		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);

		RegistrationHelper.AssertRegistration(
			assembly: fodyTestResult.Assembly,
			manifest: manifest,
			expectedModuleRegistrations:
			[
				new(
					classFullName: "MyNamespace.MyModule",
					autoInjectAttributes:
					[
						new(
							find: Find.AnyTypeOf,
							typeFullName: "MyNamespace.BaseClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.ValidClass1",
							serviceImplementationTypeFullName: "MyNamespace.ValidClass1"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.ValidClass2",
							serviceImplementationTypeFullName: "MyNamespace.ValidClass2"),
					]
				)
			]);
	}
}
