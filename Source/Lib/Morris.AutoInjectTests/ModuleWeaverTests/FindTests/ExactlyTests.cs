using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests.FindTests;

[TestClass]
public class ExactlyTests
{
	[TestMethod]
	public void WhenFindingAClass_ThenThatClassIsRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.Exactly, typeof(QualifyingClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
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
					autoInjectAttributes:
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
	public void WhenFindingAClass_ThenDescendantsOfTheClassAreNotRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.Exactly, typeof(BaseClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public abstract class BaseClass {}
			public class NonQualifyingClass : BaseClass {}
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
							typeFullName: "MyNamespace.BaseClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services: []
				)
			]);
	}

	[TestMethod]
	public void WhenFindingAnInterface_ThenClassesImplementingTheExactInterfaceAreRegistered()
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
			public class QualifyingClass : ISomeInterface {}
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
					]
				)
			]);
	}

	[TestMethod]
	public void WhenFindingAnInterface_ThenDescendantsOfClassesImplementingTheExactInterfaceAreRegistered()
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
			public abstract class BaseClass : ISomeInterface {}
			public class QualifyingClass : BaseClass {}
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
					]
				)
			]);
	}

	[TestMethod]
	public void WhenFindingAnInterface_ThenClassesImplementingADescendantOfThatInterfaceAreNotRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.Exactly, typeof(IBaseInterface), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface IBaseInterface {}
			public interface IChildInterface {}
			public class NonQualifyingClass : IChildInterface {}
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
							typeFullName: "MyNamespace.IBaseInterface",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services: []
				)
			]);
	}

}
