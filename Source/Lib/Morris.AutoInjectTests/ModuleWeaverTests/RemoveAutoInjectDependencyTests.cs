using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class RemoveAutoInjectDependencyTests
{
	[TestMethod]
	public void WhenWeavingIsSuccessful_ThenReferenceToAutoInjectShouldHaveBeenRemoved()
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

		bool isReferenced =
			fodyTestResult
			.Assembly
			.GetReferencedAssemblies()
			.Select(x => x.FullName.Split(',')[0])
			.Any(x => x == "Morris.AutoInject");
		Assert.IsFalse(isReferenced, "Morris.AutoInject should not be referenced.");
	}

	[TestMethod]
	public void WhenWeavingIsSuccessful_ThenAutoInjectAttributesShouldHaveBeenRemoved()
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

		Type module = fodyTestResult.Assembly.GetType("MyNamespace.MyModule")!;
		int attributeCount =
			module
			.GetCustomAttributes()
			.Count();
		Assert.AreEqual(0, attributeCount);
	}

	[TestMethod]
	public void WhenWeavingIsSuccessful_ThenAutoInjectFilterAttributesShouldHaveBeenRemoved()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInjectFilter("Hello")]
			public partial class MyModule
			{
			}
			""";
		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);

		Type module = fodyTestResult.Assembly.GetType("MyNamespace.MyModule")!;
		int attributeCount =
			module
			.GetCustomAttributes()
			.Count();
		Assert.AreEqual(0, attributeCount);
	}

	[TestMethod]
	public void WhenWeavingIsSuccessful_ThenNonAutoInjectAttributesShouldBePreserved()
	{
		string sourceCode =
			"""
			using Morris.AutoInject;

			namespace MyNamespace;
			[AutoInjectFilter("Hello")]
			[System.Serializable]
			[AutoInject(Find.Exactly, typeof(object), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
			public partial class MyModule
			{
			}
			""";
		WeaverExecutor.Execute(sourceCode, out Fody.TestResult? fodyTestResult, out string? manifest);

		Type module = fodyTestResult.Assembly.GetType("MyNamespace.MyModule")!;
		int attributeCount =
			module
			.GetCustomAttributes()
			.Count();
		Assert.AreEqual(1, attributeCount);

		SerializableAttribute? serializableAttribute = module.GetCustomAttribute<SerializableAttribute>();
		Assert.IsNotNull(serializableAttribute);
	}

}
