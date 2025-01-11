using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace Morris.AutoRegister.SourceGenerators.Extensions;

internal static partial class IndentedTextWriterIdentedBlockExtension
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IDisposable IndentedBlock(this IndentedTextWriter writer)
	{
		writer.Indent++;
		return new DisposableAction(() => writer.Indent--);
	}
}
