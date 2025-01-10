namespace Morris.AutoInject.SourceGeneratorsTests.RegisterSourceGeneratorTests;

[TestClass]
public sealed class RegisterTests
{
	[TestMethod]
	public void WhenClassHasAutoInjectAttribute_ThenRegisterMethodIsAdded()
	{
		string sourceCode =
			$$"""
			using Morris.AutoInject;
			namespace Tests
			{
				[AutoInject(Find.Exactly, typeof(object), RegisterAs.DiscoveredClass, WithLifetime.Scoped)]
				public partial class MyModule
				{
				}
			}
			""";

		string expectedGeneratedCode =
			$$"""
			using Microsoft.Extensions.DependencyInjection;

			namespace Tests
			{
				partial class MyModule
				{
					static partial void AfterRegisterServices(IServiceCollection services);
					public static void RegisterServices(IServiceCollection services)
					{
						AfterRegisterServices(services);
						throw new System.NotImplementedException("Morris.AutoInject.Fody has not processed this assembly.");
					}
				}
			}
			""";

		SourceGeneratorExecutor
			.AssertGeneratedCodeMatches(
				sourceCode: sourceCode,
				expectedGeneratedCode: expectedGeneratedCode
			);
	}

	[TestMethod]
	public void WhenClassHasAutoInjectFilterAttribute_ThenRegisterMethodIsAdded()
	{
		string sourceCode =
			$$"""
			using Morris.AutoInject;
			namespace Tests
			{
				[AutoInjectFilter("hello")]
				public partial class MyModule {}
			}
			""";

		string expectedGeneratedCode =
			$$"""
			using Microsoft.Extensions.DependencyInjection;

			namespace Tests
			{
				partial class MyModule
				{
					static partial void AfterRegisterServices(IServiceCollection services);
					public static void RegisterServices(IServiceCollection services)
					{
						AfterRegisterServices(services);
						throw new System.NotImplementedException("Morris.AutoInject.Fody has not processed this assembly.");
					}
				}
			}
			""";

		SourceGeneratorExecutor
			.AssertGeneratedCodeMatches(
				sourceCode: sourceCode,
				expectedGeneratedCode: expectedGeneratedCode
			);
	}
}
