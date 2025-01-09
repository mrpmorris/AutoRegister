using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class RegisterAsTests
{
	[TestMethod]
	public void WhenRegisteringAsDiscoveredClass_ThenDiscoveredClassIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.AnyTypeOf, typeof(IMarker), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
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
					autoInjectAttributes:
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
	public void WhenRegisteringAsBaseType_ThenTypeFromFindCriteriaIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.DescendantsOf, typeof(BaseClass), RegisterAs.BaseType, WithLifetime.Scoped)]
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
					autoInjectAttributes:
					[
						new(
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.BaseClass",
							registerAs: RegisterAs.BaseType,
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
	public void WhenRegisteringAsBaseClosedGenericType_ThenClosedGenericTypeFromFindCriteriaIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.DescendantsOf, typeof(BaseClass<,>), RegisterAs.BaseClosedGenericType, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public abstract class BaseClass<TKey, TValue> {}
			public class NonQualifyingClass<TValue> : BaseClass<int, TValue> {}

			public class QualifyingClass1 : BaseClass<int, string> {}
			public class QualifyingClass2 : BaseClass<int, string> {}
			public class QualifyingClass3 : NonQualifyingClass<string> {}
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
					autoInjectAttributes:
					[
						new(
							find: Find.AnyTypeOf,
							typeFullName: "MyNamespace.BaseClass`2",
							registerAs: RegisterAs.BaseType,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<int, string>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass1"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<int, string>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass2"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<int, string>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass3"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.BaseClass<int, string>",
							serviceImplementationTypeFullName: "MyNamespace.QualifyingClass4"),
					]
				)
			]);
	}

	[TestMethod]
	public void WhenRegisteringAsBaseClosedGenericType_AndFindCriteriaIsNotOpenGeneric_ThenTypeFromFindCriteriaIsUsedAsServiceType()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.DescendantsOf, typeof(BaseClass), RegisterAs.BaseClosedGenericType, WithLifetime.Scoped)]
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
					autoInjectAttributes:
					[
						new(
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace.BaseClass",
							registerAs: RegisterAs.BaseClosedGenericType,
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
