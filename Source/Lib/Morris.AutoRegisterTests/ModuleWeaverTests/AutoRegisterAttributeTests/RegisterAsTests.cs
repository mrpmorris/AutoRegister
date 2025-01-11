using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister;
using Morris.AutoRegisterTests.Helpers;

namespace Morris.AutoRegisterTests.ModuleWeaverTests.AutoRegisterAttributeTests;

[TestClass]
public class RegisterAsTests
{
	[TestMethod]
	public void WhenRegisteringAsDiscoveredClass_ThenDiscoveredClassIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.AnyTypeOf, typeof(IMarker), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface IMarker {}
			public class QualifyingClass1 : IMarker {}
			public class QualifyingClass2 : IMarker {}
			public class QualifyingClass3 : QualifyingClass1 {}
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
							typeFullName: "MyNamespace.IMarker",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.QualifyingClass1",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass1"),
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
			]);
	}

	[TestMethod]
	public void WhenRegisteringAsSearchedType_ThenTypeFromFindCriteriaIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.SearchedType, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public abstract class BaseClass {}
			public class QualifyingClass1 : BaseClass {}
			public class QualifyingClass2 : BaseClass {}
			public class QualifyingClass3 : QualifyingClass1 {}
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
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.BaseClass",
							registerAs: RegisterAs.SearchedType,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass1"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass2"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass3"),
					]
				)
			]);
	}

	[TestMethod]
	public void WhenRegisteringSearchedTypeAsClosedGeneric_AndFindCriteriaIsAClass_ThenClosedGenericTypeFromFindCriteriaIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.DescendantsOf, typeof(BaseClass<,>), RegisterAs.SearchedTypeAsClosedGeneric, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public abstract class BaseClass<TKey, TValue> {}
			public class IntBasedValue<T> : BaseClass<int, T> {}

			public class QualifyingClass1 : BaseClass<int, string> {}
			public class QualifyingClass2 : BaseClass<int, string> {}
			public class QualifyingClass3 : IntBasedValue<string> {}
			public class QualifyingClass4 : QualifyingClass3 {}
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
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.BaseClass`2",
							registerAs: RegisterAs.SearchedTypeAsClosedGeneric,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass1"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass2"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass3"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass4"),
					]
				)
			]);
	}

	[TestMethod]
	public void WhenRegisteringSearchedTypeAsClosedGeneric_AndFindCriteriaIsAnInterface_ThenClosedGenericTypeFromFindCriteriaIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.AnyTypeOf, typeof(IBaseInterface<,>), RegisterAs.SearchedTypeAsClosedGeneric, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface IBaseInterface<TKey, TValue> {}
			public interface IIntBasedValue<T> : IBaseInterface<int, T> {}

			public class QualifyingClass1 : IBaseInterface<int, string> {}
			public class QualifyingClass2 : IBaseInterface<int, string> {}
			public class QualifyingClass3 : IIntBasedValue<string> {}
			public class QualifyingClass4 : QualifyingClass3 {}
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
							typeFullName: "MyNamespace.IBaseInterface`2",
							registerAs: RegisterAs.SearchedTypeAsClosedGeneric,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.IBaseInterface<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass1"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.IBaseInterface<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass2"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.IBaseInterface<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass3"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.IBaseInterface<System.Int32, System.String>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass4"),
					]
				)
			]);
	}

	[TestMethod]
	public void WhenRegisteringSearchedTypeAsClosedGeneric_AndFindCriteriaIsNotOpenGeneric_ThenTypeFromFindCriteriaIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.SearchedTypeAsClosedGeneric, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public abstract class BaseClass {}
			public class QualifyingClass1 : BaseClass {}
			public class QualifyingClass2 : BaseClass {}
			public class QualifyingClass3 : QualifyingClass1 {}
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
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.BaseClass",
							registerAs: RegisterAs.SearchedTypeAsClosedGeneric,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass1"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass2"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass3"),
					]
				)
			]);
	}


}
