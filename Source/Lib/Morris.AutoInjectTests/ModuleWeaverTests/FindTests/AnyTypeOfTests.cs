using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests.FindTests;

[TestClass]
public class AnyTypeOfTests
{
	[TestMethod]
	public void WhenFindingAClass_ThenTheSameClassIsRegistered()
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
	public void WhenFindingAClass_ThenDescendantClassesAreRegistered()
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

	[TestMethod]
	public void WhenFindingAnInterface_ThenClassesImplementingTheInterfaceAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.AnyTypeOf, typeof(ISomeInterface), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface ISomeInterface {}
			public class SomeClass : ISomeInterface {}
			public class NonQualifyingClass {}
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
							typeFullName: "MyNamespace.ISomeInterface",
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
	public void WhenFindingAnInterface_ThenClassesImplementingADescendantInterfaceAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.AnyTypeOf, typeof(IBaseInterface), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface IBaseInterface {}
			public interface IChildInterface : IBaseInterface {}
			public class SomeClass : IChildInterface {}
			public class NonQualifyingClass {}
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
							typeFullName: "MyNamespace.IBaseInterface",
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
	public void WhenFindingOpenGenericClass_ThenClosedGenericDescendantsOfThatClassAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.AnyTypeOf, typeof(GenericBase<>), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public class GenericBase<T> {}
			public class QualifyingClass : GenericBase<int> {}
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
							typeFullName: "MyNamespace.GenericBase`1",
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
			]
		);
	}
}
