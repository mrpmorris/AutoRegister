using Mono.Cecil;
using System.Text.RegularExpressions;

namespace Morris.AutoRegister.Fody;

internal class AutoRegisterFilterAttributeData
{
	public string? ServiceImplementationTypeFilter { get; private set; }

	private Regex ServiceImplementationRegex;

	public AutoRegisterFilterAttributeData(string? serviceImplementationTypeFilter)
	{
		ServiceImplementationTypeFilter = serviceImplementationTypeFilter;
		ServiceImplementationRegex = new Regex(serviceImplementationTypeFilter, RegexOptions.Compiled);
	}

	public bool Matches(TypeDefinition typeDefinition) =>
		ServiceImplementationRegex.IsMatch(typeDefinition.FullName);
}
