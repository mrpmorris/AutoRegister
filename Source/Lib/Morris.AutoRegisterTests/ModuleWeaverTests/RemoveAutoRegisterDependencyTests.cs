using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Morris.AutoRegisterTests.ModuleWeaverTests;

public class RemoveAutoRegisterDependencyTests
{
	[Fact]
	public void WhenWeavingIsSuccessful_ThenReferenceToAutoRegisterShouldHaveBeenRemoved()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.Exactly, typeof(object), RegisterAs.ImplementingClass, WithLifetime.Scoped)]
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
			.Any(x => x == "Morris.AutoRegister");
		Assert.False(isReferenced, "Morris.AutoRegister should not be referenced.");
	}

	[Fact]
	public void WhenWeavingIsSuccessful_ThenAutoRegisterAttributesShouldHaveBeenRemoved()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegister(Find.Exactly, typeof(object), RegisterAs.ImplementingClass, WithLifetime.Scoped)]
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
		Assert.Equal(0, attributeCount);
	}

	[Fact]
	public void WhenWeavingIsSuccessful_ThenAutoRegisterFilterAttributesShouldHaveBeenRemoved()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegisterFilter("Hello")]
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
		Assert.Equal(0, attributeCount);
	}

	[Fact]
	public void WhenWeavingIsSuccessful_ThenNonAutoRegisterAttributesShouldBePreserved()
	{
		string sourceCode =
			"""
			using Morris.AutoRegister;

			namespace MyNamespace;
			[AutoRegisterFilter("Hello")]
			[System.Serializable]
			[AutoRegister(Find.Exactly, typeof(object), RegisterAs.ImplementingClass, WithLifetime.Scoped)]
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
		Assert.Equal(1, attributeCount);

		SerializableAttribute? serializableAttribute = module.GetCustomAttribute<SerializableAttribute>();
		Assert.NotNull(serializableAttribute);
	}

}
