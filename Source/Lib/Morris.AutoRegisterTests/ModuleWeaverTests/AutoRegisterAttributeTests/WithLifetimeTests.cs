using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister;
using Morris.AutoRegisterTests.Helpers;

namespace Morris.AutoRegisterTests.ModuleWeaverTests.AutoRegisterAttributeTests;

[TestClass]
public class WithLifetimeTests
{
	[TestMethod]
	public void WhenSingleton_ThenRegistersAsSingleton()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.Exactly, typeof(QualifyingClass), RegisterAs.DiscoveredClass, WithLifetime.Singleton)]
			public partial class MyModule
			{
			}

			public class QualifyingClass {}
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
							find: Find.Exactly,
							typeFullName: "MyNamespace.QualifyingClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Singleton)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Singleton,
							serviceTypeFullName: "MyNamespace.QualifyingClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass")
					]
				)
			]);
	}

	[TestMethod]
	public void WhenScoped_ThenRegistersAsScoped()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.Exactly, typeof(QualifyingClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public class QualifyingClass {}
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
							find: Find.Exactly,
							typeFullName: "MyNamespace.QualifyingClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.QualifyingClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass")
					]
				)
			]);
	}

	[TestMethod]
	public void WhenTransient_ThenRegistersAsTransient()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.Exactly, typeof(QualifyingClass), RegisterAs.DiscoveredClass, WithLifetime.Transient)]
			public partial class MyModule
			{
			}

			public class QualifyingClass {}
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
							find: Find.Exactly,
							typeFullName: "MyNamespace.QualifyingClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Transient)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Transient,
							serviceTypeFullName: "MyNamespace.QualifyingClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass")
					]
				)
			]);
	}

}
