using Microsoft.CodeAnalysis;
using Morris.AutoRegister.SourceGenerators.Extensions;
using System.CodeDom.Compiler;
using System.Collections.Immutable;

namespace Morris.AutoRegister.SourceGenerators;

[Generator]
public class RegisterSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<ImmutableArray<(string? NamespaceName, string Name)>> classesWithAutoRegisterAttribute =
			context
			.SyntaxProvider
			.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: "Morris.AutoRegister.AutoRegisterAttribute",
				predicate: static (_, _) => true,
				transform: static (context, token) =>
				{
					ISymbol targetSymbol = context.TargetSymbol;
					string? namespaceName =
						targetSymbol.ContainingNamespace.IsGlobalNamespace
						? null
						: targetSymbol.ContainingNamespace.Name;
					return (namespaceName, targetSymbol.Name);
				}
			)
			.Collect();

		IncrementalValueProvider<ImmutableArray<(string? NamespaceName, string Name)>> classesWithAutoRegisterFilterAttribute =
			context
			.SyntaxProvider
			.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: "Morris.AutoRegister.AutoRegisterFilterAttribute",
				predicate: static (_, _) => true,
				transform: static (context, token) =>
				{
					ISymbol targetSymbol = context.TargetSymbol;
					string? namespaceName =
						targetSymbol.ContainingNamespace.IsGlobalNamespace
						? null
						: targetSymbol.ContainingNamespace.Name;
					return (namespaceName, targetSymbol.Name);
				}
			)
			.Collect();

		IncrementalValueProvider<ImmutableArray<(string? NamespaceName, string Name)>> allClassesWithAutoRegisterAttributes =
			classesWithAutoRegisterAttribute
			.Combine(classesWithAutoRegisterFilterAttribute)
			.Select(static (combined, _) =>
			{
				var (autoRegisterClasses, autoRegisterFilterClasses) = combined;
				return autoRegisterClasses.Concat(autoRegisterFilterClasses).Distinct().ToImmutableArray();
			});


		context.RegisterSourceOutput(allClassesWithAutoRegisterAttributes, static (context, items) =>
		{
			using var output = new StringWriter();
			using var writer = new IndentedTextWriter(output);

			writer.WriteLine("using Microsoft.Extensions.DependencyInjection;");

			foreach (var item in items)
				GenerateCodeForTarget(writer, item);

			writer.Flush();
			context.AddSource($"Morris.AutoRegister.{nameof(RegisterSourceGenerator)}.g.cs", output.ToString());
		});
	}

	private static void GenerateCodeForTarget(
		IndentedTextWriter writer,
		(string? NamespaceName, string Name) item)
	{
		writer.AddBlankLine();
		IDisposable? namespaceBlock =
			item.NamespaceName is not null
			? writer.CodeBlock($"namespace {item.NamespaceName}")
			: null;

		using (writer.CodeBlock($"partial class {item.Name}"))
		{
			writer.WriteLine("static partial void AfterRegisterServices(IServiceCollection services);");
			using (writer.CodeBlock("public static void RegisterServices(IServiceCollection services)"))
			{
				writer.WriteLine("AfterRegisterServices(services);");
				writer.WriteLine("throw new System.NotImplementedException(\"Morris.AutoRegister.Fody has not processed this assembly.\");");
			}
		}

		namespaceBlock?.Dispose();
	}
}
