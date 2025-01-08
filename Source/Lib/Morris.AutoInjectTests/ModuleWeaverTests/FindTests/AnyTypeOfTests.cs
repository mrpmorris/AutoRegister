using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests.FindTests;

[TestClass]
public class AnyTypeOfTests
{
	[TestMethod]
	public void WhenCandidateIsTheSameClassAsInTheCriteria_ThenTheCandidateIsRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.AnyTypeOf, typeof(SomeClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public class SomeClass {}
			""";

		WeaverExecutor.Execute(sourceCode, out Fody.TestResult fodyTestResult, out string? manifest);

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
							typeFullName: "MyNamespace.SomeClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.SomeClass",
							serviceImplementationTypeFullName: "MyNamespace.SomeClass")
					])
			]
		);
	}

	[TestMethod]
	public void WhenCandidateIsADescendantOfTheClassInTheCriteria_ThenTheCandidateIsRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.AnyTypeOf, typeof(SomeClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public class SomeClass {}
			public class SomeChildClass : SomeClass {}
			""";

		WeaverExecutor.Execute(sourceCode, out Fody.TestResult fodyTestResult, out string? manifest);

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
							typeFullName: "MyNamespace.SomeClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.SomeClass",
							serviceImplementationTypeFullName: "MyNamespace.SomeClass"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.SomeChildClass",
							serviceImplementationTypeFullName: "MyNamespace.SomeChildClass"),
					])
			]
		);
	}

}
