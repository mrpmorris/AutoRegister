﻿using Morris.AutoInject;
using Morris.AutoInjectTests.Helpers;

namespace Morris.AutoInjectTests.ModuleWeaverTests.AutoInjectAttributeTests;

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
}