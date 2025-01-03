using Fody;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInject.Fody;
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


	public static void Execute(string sourceCode, out Fody.TestResult testResult, out string? manifest)
	{
		var unitTestSyntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
		var compilation = CSharpCompilation.Create(
			assemblyName: "Test",
			syntaxTrees: [unitTestSyntaxTree],
			references: Basic.Reference.Assemblies.Net90.References
				.All
				.Union([AutoInjectMetadataReference, MSDependencyInjectionMetadataReference]),
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true)
		);
		AssertNoCompileDiagnostics(compilation);

		string projectFilePath = Path.Combine(
			Path.GetTempPath(),
			$"{Guid.NewGuid()}.csproj"
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
		}
		finally
		{
			if (File.Exists(manifestFilePath))
				File.Delete(manifestFilePath);
			if (File.Exists(assemblyFilePath))
				File.Delete(assemblyFilePath);
		}
	}

	private static void AssertNoCompileDiagnostics(CSharpCompilation compilation)
	{
		ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
		if (!diagnostics.Any())
			return;

		var builder = new StringBuilder();
		foreach (var diagnostic in diagnostics)
			builder.AppendLine(diagnostic.ToString());

		Assert.Fail("The following compiler errors were found:\r\n" + builder.ToString());
	}

}


