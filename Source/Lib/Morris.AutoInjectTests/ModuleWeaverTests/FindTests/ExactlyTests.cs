using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests.FindTests;

[TestClass]
public class ExactlyTests
{
	[TestMethod]
	public void WhenCriteriaIsClass_ThenOnlyExactMatchingClassIsRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.Exactly, typeof(SomeClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public class SomeClass {}
			public class SomeChildClass : SomeClass {}
			public class SomeUnrelatedClass {}
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
							find: Find.Exactly,
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
			]);
	}

	[TestMethod]
	public void WhenCriteriaIsInterface_ThenOnlyExactMatchingInterfaceIsRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.Exactly, typeof(ISomeInterface), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface ISomeInterface {}
			public interface ISomeChildInterface : ISomeInterface {}

			public class QualifyingClass : ISomeInterface {}
			public class NonQualifyingClass : ISomeChildInterface {}
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
							find: Find.Exactly,
							typeFullName: "MyNamespace.ISomeInterface",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.QualifyingClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass")
					])
			]);
	}
}
