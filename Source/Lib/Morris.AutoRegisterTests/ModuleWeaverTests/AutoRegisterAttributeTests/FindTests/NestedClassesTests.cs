using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister;
using Morris.AutoRegisterTests.Helpers;

namespace Morris.AutoRegisterTests.ModuleWeaverTests.AutoRegisterAttributeTests.FindTests;

[TestClass]
public class NestedClassesTests
{
	[TestMethod]
	public void WhenAClassIsNested_ThenThatClassCanStillBeRegistered()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.Exactly, typeof(IScoped), RegisterAs.ImplementingClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}

			public interface IScoped {}
			internal static class OuterClass
			{
				private class QualifyingClass : IScoped {}
			}
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
							typeFullName: "MyNamespace.IScoped",
							registerAs: RegisterAs.ImplementingClass,
							withLifetime: WithLifetime.Scoped)
					],
					services:
					[
						new(
							lifetime: ServiceLifetime.Scoped,
							serviceTypeFullName: "MyNamespace.OuterClass+QualifyingClass",
							serviceImplementationTypeFullName: "MyNamespace.OuterClass+QualifyingClass")
					]
				)
			]);
	}
}
