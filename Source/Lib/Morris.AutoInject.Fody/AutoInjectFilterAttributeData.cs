using Mono.Cecil;
using System.Text.RegularExpressions;

namespace Morris.AutoInject.Fody;

internal class AutoInjectFilterAttributeData
{
	public string? ServiceImplementationFilter { get; private set; }

	private Regex ServiceImplementationRegex;

	public AutoInjectFilterAttributeData(string? serviceImplementationRegex)
	{
		ServiceImplementationFilter = serviceImplementationRegex;
		ServiceImplementationRegex = new Regex(serviceImplementationRegex, RegexOptions.Compiled);
	}

	public bool Matches(TypeDefinition typeDefinition) =>
		ServiceImplementationRegex.IsMatch(typeDefinition.FullName);
}
