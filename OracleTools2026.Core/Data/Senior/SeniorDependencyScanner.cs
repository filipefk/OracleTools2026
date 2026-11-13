using OracleTools2026.Models.Senior;
using System.Text.RegularExpressions;

namespace OracleTools2026.Data.Senior;

// Monta o dependencias.yaml a partir do fonte das regras dos webservices e aponta possíveis segredos
// gravados no fonte (avisos, que não bloqueiam a exportação).
public sealed partial class SeniorDependencyScanner
{
    private const string StandardServicePrefix = "com.senior.";

    public SeniorDependencyScan Scan(IReadOnlyList<SeniorService> services)
    {
        // Do nome mais longo para o mais curto, para "a.b.c" ganhar de "a.b" ao resolver "a.b.c.Porta".
        var serviceNames = services.Select(s => s.Name).OrderByDescending(n => n.Length).ToList();
        var dependencies = new List<SeniorDependency>();
        var warnings = new List<string>();

        foreach (var port in services.SelectMany(s => s.Ports)
                     .Where(p => p.RuleSource is not null)
                     .OrderBy(p => p.QualifiedName, StringComparer.Ordinal))
        {
            var source = port.RuleSource!;

            var calls = InternalServiceRegex().Matches(source)
                .Select(m => ResolveCall(m.Groups[1].Value, serviceNames))
                .Where(c => c.Name != port.QualifiedName)
                .DistinctBy(c => c.Name)
                .OrderBy(c => c.IsStandard)
                .ThenBy(c => c.Name, StringComparer.Ordinal)
                .ToList();

            var tables = CustomTableRegex().Matches(source)
                .Select(m => m.Value.ToUpperInvariant())
                .Distinct()
                .Order(StringComparer.Ordinal)
                .ToList();

            dependencies.Add(new SeniorDependency(port.QualifiedName, calls, tables));

            var lines = source.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (SecretRegex().Match(lines[i]) is { Success: true } match)
                {
                    warnings.Add($"{port.QualifiedName}, linha {i + 1}: possível segredo no fonte ({match.Groups[1].Value} = \"...\")");
                }
            }
        }

        return new SeniorDependencyScan(dependencies, warnings);
    }

    // "interno.senior.com.olfar.tms.FATNumero" vira "senior.com.olfar.tms/FATNumero" quando o serviço foi
    // exportado; os demais (padrão Senior ou fora da exportação) ficam como estão no fonte.
    private static SeniorServiceCall ResolveCall(string name, IReadOnlyList<string> serviceNames)
    {
        var service = serviceNames.FirstOrDefault(s => name.StartsWith(s + ".", StringComparison.OrdinalIgnoreCase));
        var resolved = service is null ? name : $"{service}/{name[(service.Length + 1)..]}";
        return new SeniorServiceCall(resolved, name.StartsWith(StandardServicePrefix, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(@"\bDefinir\s+interno\.([A-Za-z_][\w.]*\w)", RegexOptions.IgnoreCase)]
    private static partial Regex InternalServiceRegex();

    [GeneratedRegex(@"\bUSU_T\w+", RegexOptions.IgnoreCase)]
    private static partial Regex CustomTableRegex();

    [GeneratedRegex(@"\w*(senha|password|passwd|pwd|token|secret|apikey|api_key)\w*\s*=\s*""[^""]+""", RegexOptions.IgnoreCase)]
    private static partial Regex SecretRegex();
}
