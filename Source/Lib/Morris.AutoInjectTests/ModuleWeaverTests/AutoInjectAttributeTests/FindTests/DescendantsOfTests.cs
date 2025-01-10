using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests.AutoInjectAttributeTests.FindTests;

[TestClass]
public class DescendantsOfTests
{
	[TestMethod]
	public void WhenFindingAClass_ThenTheSameClassIsNotRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.DescendantsOf, typeof(SomeClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
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
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.SomeClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services: []
				)
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
			[AutoInject(Find.DescendantsOf, typeof(SomeClass), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public abstract class SomeClass {}
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
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.SomeClass",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.SomeChildClass",
							serviceImplementationTypeFullName: "MyNamespace.SomeChildClass"),
					]
				)
			]
		);
	}

	[TestMethod]
	public void WhenFindingAnInterface_ThenClassesImplementingTheExactInterfaceAreNotRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.DescendantsOf, typeof(ISomeInterface), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface ISomeInterface {}
			public class SomeClass : ISomeInterface {}
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
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.ISomeInterface",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services: []
				)
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
			[AutoInject(Find.DescendantsOf, typeof(IBaseInterface), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface IBaseInterface {}
			public interface IChildInterface : IBaseInterface {}
			public class SomeClass : IChildInterface {}
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
							find: Find.DescendantsOf,
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
					]
				)
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
			[AutoInject(Find.DescendantsOf, typeof(GenericBase<>), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
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
							find: Find.DescendantsOf,
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
					]
				)
			]
		);
	}

	[TestMethod]
	public void WhenFindingAnOpenGenericInterface_ThenClassesImplementingDescendantsOfThatInterfaceAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.DescendantsOf, typeof(IGenericInterface<>), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface IGenericInterface<T> {}
			public class QualifyingClass1 : IGenericInterface<int> {}

			public interface IGenericChildInterface<T> : IGenericInterface<T> {}
			public class QualifyingClass2 : IGenericChildInterface<int> {}

			public interface INonGenericInterface : IGenericChildInterface<int> {}
			public class QualifyingClass3: INonGenericInterface {}
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
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.IGenericInterface`1",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.QualifyingClass2",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass2"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.QualifyingClass3",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass3"),
					]
				)
			]
		);
	}
}
