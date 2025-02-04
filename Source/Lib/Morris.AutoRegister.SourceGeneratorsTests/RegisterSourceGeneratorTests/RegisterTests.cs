namespace Morris.AutoRegister.SourceGeneratorsTests.RegisterSourceGeneratorTests;

[TestClass]
public sealed class RegisterTests
{
	[TestMethod]
	public void WhenClassHasAutoRegisterAttribute_ThenRegisterMethodIsAdded()
	{
		string sourceCode =
			$$"""
			using Morris.AutoRegister;
			namespace My.Tests
			{
				[AutoRegister(Find.Exactly, typeof(object), RegisterAs.ImplementingClass, WithLifetime.Scoped)]
				public partial class MyModule
				{
				}
			}
			""";

		string expectedGeneratedCode =
			$$"""
			using Microsoft.Extensions.DependencyInjection;

			namespace My.Tests
			{
				partial class MyModule
				{
					static partial void AfterRegisterServices(IServiceCollection services);
					public static void RegisterServices(IServiceCollection services)
					{
						AfterRegisterServices(services);
						throw new System.NotImplementedException("Morris.AutoRegister.Fody has not processed this assembly.");
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
	public void WhenClassHasAutoRegisterFilterAttribute_ThenRegisterMethodIsAdded()
	{
		string sourceCode =
			$$"""
			using Morris.AutoRegister;
			namespace Tests
			{
				[AutoRegisterFilter("hello")]
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
						throw new System.NotImplementedException("Morris.AutoRegister.Fody has not processed this assembly.");
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
