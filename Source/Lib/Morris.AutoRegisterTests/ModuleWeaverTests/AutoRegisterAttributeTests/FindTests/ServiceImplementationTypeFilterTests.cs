using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister;
using Morris.AutoRegisterTests.Helpers;

namespace Morris.AutoRegisterTests.ModuleWeaverTests.AutoRegisterAttributeTests.FindTests;

public class ServiceImplementationTypeFilterTests
{
	[Fact]
	public void WhenAServiceImplementationTypeFilterIsSpecified_ThenOnlyServiceTypesMatchingThatFilterAreRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace1
			{
				[AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.ImplementingClass, WithLifetime.Scoped, ServiceImplementationTypeFilter = @"^MyNamespace2\.")]
				public partial class MyModule
				{
				}

				public abstract class BaseClass {}
				public class InvalidClass1 : BaseClass {}
				public class InvalidClass2 : BaseClass {}
			}

			namespace MyNamespace2
			{
				public class ValidClass1 : MyNamespace1.BaseClass {}
				public class ValidClass2 : MyNamespace1.BaseClass {}
			}
			
			""";

		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);

		RegistrationHelper.AssertRegistration(
			assembly: fodyTestResult.Assembly,
			manifest: manifest,
			expectedModuleRegistrations:
			[
				new(
					classFullName: "MyNamespace1.MyModule",
					autoRegisterAttributes:
					[
						new(
							find: Find.DescendantsOf,
							typeFullName: "MyNamespace1.BaseClass",
							registerAs: RegisterAs.ImplementingClass,
							withLifetime: WithLifetime.Scoped,
							serviceImplementationTypeFilter: @"^MyNamespace2\.")
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace2.ValidClass1",
							serviceImplementationTypeFullName: "MyNamespace2.ValidClass1"),
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace2.ValidClass2",
							serviceImplementationTypeFullName: "MyNamespace2.ValidClass2")
					]
				)
			]);
	}
}
