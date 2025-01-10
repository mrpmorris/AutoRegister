using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;
using System.Reflection;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class ModuleTests
{
	[TestMethod]
	public void WhenClassHasAutoInjectAttribute_ThenClassAppearsInManifest()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInject(Find.Exactly, typeof(object), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
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
					autoInjectAttributes: [
						new(
							find: Find.Exactly,
							typeFullName: "System.Object",
							registerAs: RegisterAs.DiscoveredClass,
							withLifetime: WithLifetime.Scoped)
					],
					services: []
				)
			]
		);
	}

	[TestMethod]
	public void WhenClassHasAutoInjectFilterAttribute_ThenClassAppearsInManifest()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace1
			{
				[AutoInjectFilter("SomeFilter")]
				public partial class MyModule
				{
				}
			}

			namespace MyNamespace2
			{
				[AutoInjectFilter("SomeFilter")]
				public partial class MyModule
				{
				}
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
					autoInjectAttributes: [],
					services: []
				),
				new(
					classFullName: "MyNamespace2.MyModule",
					autoInjectAttributes: [],
					services: []
				),
			]
		);
	}

	[TestMethod]
	public void WhenClassHasNoAutoInjectAttributes_ThenClassDoesNotAppearInManifest()
	{
		string sourceCode =
			"""
			namespace MyNamespace1
			{
				public partial class MyModule
				{
				}
			}

			namespace MyNamespace2
			{
				public partial class MyModule
				{
				}
			}
			""";

		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);

		RegistrationHelper.AssertRegistration(
			assembly: fodyTestResult.Assembly,
			manifest: manifest,
			expectedModuleRegistrations: []
		);
	}

	[TestMethod]
	public void WhenClassHasAfterRegisterServicesMethod_ThenThatMethodIsCalledFromRegisterServices()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;
			using Microsoft.Extensions.DependencyInjection;

			namespace MyNamespace;
			[AutoInject(Find.Exactly, typeof(object), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}
			""";

		string partialMethodsSourceCode =
			"""
			using Microsoft.Extensions.DependencyInjection;

			namespace MyNamespace;
			public partial class MyModule
			{
				static partial void AfterRegisterServices(IServiceCollection services)
				{
					throw new System.ApplicationException("AfterRegisterServices was executed.");
				}
			}
			""";

		WeaverExecutor.Execute(
			sourceCode: sourceCode,
			testResult: out Fody.TestResult? fodyTestResult,
			manifest: out string? manifest,
			partialMethodsSourceCode: partialMethodsSourceCode
		);

		Type moduleType = fodyTestResult.Assembly.GetType("MyNamespace.MyModule")!;
		MethodInfo registerServicesMethod =
			moduleType
			.GetMethod("RegisterServices", BindingFlags.Public | BindingFlags.Static)!;

		var services = new ServiceCollection();
		try
		{
			registerServicesMethod.Invoke(
				obj: null,
				parameters: [services]
			);
			Assert.Fail("No exception was thrown.");
		}
		catch (TargetInvocationException invocationException)
		{
			var applicationException = invocationException?.InnerException as ApplicationException;
			Assert.IsNotNull(applicationException, "Expected inner exception to be ApplicationException.");
			Assert.AreEqual("AfterRegisterServices was executed.", applicationException.Message);
		}
		
	}
}
