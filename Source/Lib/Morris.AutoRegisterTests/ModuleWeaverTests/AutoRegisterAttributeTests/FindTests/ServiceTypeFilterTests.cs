using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister;
using Morris.AutoRegisterTests.Helpers;

namespace Morris.AutoRegisterTests.ModuleWeaverTests.AutoRegisterAttributeTests.FindTests;

[TestClass]
public class ServiceTypeFilterTests
{
	[TestMethod]
	public void WhenAServiceTypeFilterIsSpecified_ThenOnlyServiceTypesMatchingThatFilterAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.ImplementingClass, WithLifetime.Scoped, ServiceTypeFilter = "ValidClass")]
			public partial class MyModule
			{
			}

			public abstract class BaseClass {}
			public class ValidClass1 : BaseClass {}
			public class ValidClass2 : BaseClass {}
			public class InvalidClass1 : BaseClass {}
			public class InvalidClass2 : BaseClass {}
			
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
							registerAs: RegisterAs.ImplementingClass,
							withLifetime: WithLifetime.Scoped,
							serviceTypeFilter: "ValidClass")
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
							serviceImplementationTypeFullName: "MyNamespace.ValidClass2")
					]
				)
			]);
	}
}
