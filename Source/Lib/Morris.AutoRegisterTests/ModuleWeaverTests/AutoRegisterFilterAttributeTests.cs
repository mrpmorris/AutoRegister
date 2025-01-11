using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister;
using Morris.AutoRegisterTests.Helpers;

namespace Morris.AutoRegisterTests.ModuleWeaverTests;

[TestClass]
public class AutoRegisterFilterAttributeTests
{
	[TestMethod]
	public void WhenModuleHasAnAutoRegisterFilterAttribute_ThenOnlyCandidatesMatchingTheFilterAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegisterFilter(@"\.ValidClass\d+")]
			[AutoRegister(Find.AnyTypeOf, typeof(BaseClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
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
					autoRegisterAttributes:
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
