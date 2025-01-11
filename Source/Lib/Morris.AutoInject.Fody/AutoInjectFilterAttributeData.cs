using Mono.Cecil;
using System.Text.RegularExpressions;

namespace Morris.AutoRegister.Fody;

internal class AutoRegisterFilterAttributeData
{
	public string? ServiceImplementationFilter { get; private set; }

	private Regex ServiceImplementationRegex;

	public AutoRegisterFilterAttributeData(string? serviceImplementationRegex)
	{
		ServiceImplementationFilter = serviceImplementationRegex;
		ServiceImplementationRegex = new Regex(serviceImplementationRegex, RegexOptions.Compiled);
	}

	public bool Matches(TypeDefinition typeDefinition) =>
		ServiceImplementationRegex.IsMatch(typeDefinition.FullName);
}
