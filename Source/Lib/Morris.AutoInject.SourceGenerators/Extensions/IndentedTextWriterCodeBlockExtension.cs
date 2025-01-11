using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace Morris.AutoRegister.SourceGenerators.Extensions;

internal static class IndentedTextWriterCodeBlockExtension
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IDisposable CodeBlock(this IndentedTextWriter writer, string? prefix = null)
	{
		if (prefix is not null)
			writer.WriteLine(prefix);

		writer.WriteLine("{");
		writer.Indent++;
		return new DisposableAction(() =>
		{
			writer.Indent--;
			writer.WriteLine("}");
		});
	}
}
