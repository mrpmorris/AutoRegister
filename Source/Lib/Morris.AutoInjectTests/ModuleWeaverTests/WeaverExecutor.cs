using Fody;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInject.Fody;
using Morris.AutoInject.SourceGenerators;
using Morris.AutoInjectTests.Extensions;
using System.Collections.Immutable;
using System.Text;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

internal static class WeaverExecutor
{
	private static readonly MetadataReference AutoInjectMetadataReference =
		MetadataReference
		.CreateFromFile(typeof(AutoInjectAttribute).Assembly.Location);

	private static readonly MetadataReference MSDependencyInjectionMetadataReference =
		MetadataReference
		.CreateFromFile(typeof(ServiceLifetime).Assembly.Location);


	public static void Execute(
		string sourceCode,
		out Fody.TestResult testResult,
		out string? manifest,
		bool assertNoDiagnosticsOutput = true,
		string? partialMethodsSourceCode = null)
	{
		Guid uniqueId = Guid.NewGuid();
		SyntaxTree unitTestSyntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

		ImmutableArray<SyntaxTree> generatedSourceSyntaxTrees = GetGeneratedSourceSyntaxTrees(unitTestSyntaxTree, uniqueId);

		SyntaxTree? partialMethodsSyntaxTree =
			partialMethodsSourceCode is null
			? null
			: CSharpSyntaxTree.ParseText(partialMethodsSourceCode);

		ImmutableArray<SyntaxTree> allSyntaxTrees =
			generatedSourceSyntaxTrees
			.Append(unitTestSyntaxTree)
			.Append(partialMethodsSyntaxTree!)
			.Where(x => x != null)
			.ToImmutableArray();

		CSharpCompilation compilation = Compile(
			assemblyName: "UnitTest",
			uniqueId: uniqueId,
			syntaxTrees: allSyntaxTrees
		);
		AssertNoCompileDiagnostics(compilation);

		string projectFilePath = Path.Combine(
			Path.GetTempPath(),
			$"{uniqueId}.csproj"
		);
		string manifestFilePath = Path.ChangeExtension(projectFilePath, "Morris.AutoInject.manifest");
		string assemblyFilePath = Path.ChangeExtension(projectFilePath, "Morris.AutoInject.Tests.dll");

		try
		{
			compilation.Emit(assemblyFilePath);
			var weaver = new ModuleWeaver() {
				ProjectFilePath = projectFilePath
			};
			testResult = weaver.ExecuteTestRun(assemblyFilePath);
			manifest =
				!File.Exists(manifestFilePath)
				? null
				: File.ReadAllText(manifestFilePath);
			if (assertNoDiagnosticsOutput)
				testResult.AssertNoDiagnosticsOutput();
		}
		finally
		{
			if (File.Exists(manifestFilePath))
				File.Delete(manifestFilePath);
			if (File.Exists(assemblyFilePath))
				File.Delete(assemblyFilePath);
		}
	}

	private static ImmutableArray<SyntaxTree> GetGeneratedSourceSyntaxTrees(
		SyntaxTree unitTestSyntaxTree,
		Guid uniqueId)
	{
		var compilation = CSharpCompilation.Create(
			assemblyName: $"Roslyn{uniqueId}",
			syntaxTrees: [unitTestSyntaxTree],
			references: Basic.Reference.Assemblies.Net90.References
				.All
				.Union([AutoInjectMetadataReference, MSDependencyInjectionMetadataReference]),
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true)
		);

		var registerSourceGenerator = new RegisterSourceGenerator().AsSourceGenerator();
		var driver = CSharpGeneratorDriver
			.Create(registerSourceGenerator)
			.RunGenerators(compilation);
		GeneratorDriverRunResult driverRunResult = driver.GetRunResult();
		GeneratorRunResult runResult = driverRunResult.Results.Single();

		ImmutableArray<SyntaxTree> result = runResult.GeneratedSources.Select(x => x.SyntaxTree).ToImmutableArray();
		return result;
	}

	private static CSharpCompilation Compile(
		string assemblyName,
		Guid uniqueId,
		ImmutableArray<SyntaxTree> syntaxTrees)
	{
		CSharpCompilation result = CSharpCompilation
			.Create(
				assemblyName: $"{assemblyName}{uniqueId}",
				syntaxTrees: syntaxTrees,
				references: Basic.Reference.Assemblies.Net90.References
					.All
					.Union([AutoInjectMetadataReference, MSDependencyInjectionMetadataReference]),
				options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true)
			);

		return result;
	}

	private static void AssertNoCompileDiagnostics(CSharpCompilation compilation)
	{
		ImmutableArray<Diagnostic> diagnostics =
			compilation
			.GetDiagnostics()
			.Where(x => x.DefaultSeverity != DiagnosticSeverity.Hidden)
			.ToImmutableArray();
		if (!diagnostics.Any())
			return;

		var builder = new StringBuilder();
		foreach (var diagnostic in diagnostics)
			builder.AppendLine(diagnostic.ToString());

		Assert.Fail("The following compiler errors were found:\r\n" + builder.ToString());
	}

}


