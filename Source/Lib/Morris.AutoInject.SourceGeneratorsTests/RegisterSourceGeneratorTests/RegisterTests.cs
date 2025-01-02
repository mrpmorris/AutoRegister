﻿namespace Morris.AutoInject.SourceGeneratorsTests.RegisterSourceGeneratorTests;

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
				[AutoInject(Find.DescendantsOf, typeof(System.Object), RegisterAs.BaseType, WithLifetime.Scoped)]
				public partial class MyModule {}
			}
			""";

		string expectedGeneratedCode =
			$$"""
			using Microsoft.Extensions.DependencyInjection;

			namespace Tests
			{
				static partial void AfterRegisterServices(IServiceCollection services);
				public static void RegisterServices(IServiceCollection services)
				{
					throw new System.NotImplementedException("Fody weaver has not processed this assembly.");
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
