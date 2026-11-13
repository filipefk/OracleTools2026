using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OracleTools2026.Formatting;

// Escritor YAML simples (sem dependência externa) no formato dos arquivos do repositório Senior: chaves
// em bloco, listas com "- " e itens inline "{chave: valor, ...}". Linhas terminam com LF.
public sealed partial class YamlText
{
    private readonly StringBuilder _text = new();

    public YamlText Comment(string comment)
    {
        foreach (var line in comment.ReplaceLineEndings("\n").Split('\n'))
        {
            _text.Append("# ").Append(line).Append('\n');
        }

        return this;
    }

    public YamlText BlankLine()
    {
        _text.Append('\n');
        return this;
    }

    // Valor escalar ou lista inline; o comentário (ex.: coluna de origem) vai no fim da linha.
    public YamlText Value(string key, object? value, string? comment = null, int indent = 0)
    {
        Indent(indent).Append(Key(key)).Append(": ").Append(Format(value));
        return EndLine(comment);
    }

    public YamlText Section(string key, int indent = 0)
    {
        Indent(indent).Append(Key(key)).Append(':');
        return EndLine(null);
    }

    public YamlText Item(object? value, string? comment = null, int indent = 2)
    {
        Indent(indent).Append("- ").Append(Format(value));
        return EndLine(comment);
    }

    // Início de um item de lista em bloco ("- chave: valor"); as demais chaves do item usam indent + 2.
    public YamlText MapItem(string key, object? value, int indent = 2)
    {
        Indent(indent).Append("- ").Append(Key(key)).Append(": ").Append(Format(value));
        return EndLine(null);
    }

    public YamlText EmptyMap(string key, int indent = 0)
    {
        Indent(indent).Append(Key(key)).Append(": {}");
        return EndLine(null);
    }

    // Texto de várias linhas em bloco literal ("|-"), legível no diff.
    public YamlText Block(string key, string text, int indent = 0)
    {
        if (text.Length == 0)
        {
            return Value(key, "", null, indent);
        }

        var lines = text.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
        var firstLine = lines.FirstOrDefault(l => l.Trim().Length > 0) ?? "";
        var indicator = firstLine.StartsWith(' ') || firstLine.StartsWith('\t') ? "2" : "";
        Indent(indent).Append(Key(key)).Append(": |").Append(indicator).Append('-').Append('\n');
        foreach (var line in lines)
        {
            if (line.Length > 0)
            {
                Indent(indent + 2).Append(line);
            }

            _text.Append('\n');
        }

        return this;
    }

    public YamlText InlineMapItem(IEnumerable<(string Key, object? Value)> entries, int indent = 2)
    {
        Indent(indent).Append("- {")
            .AppendJoin(", ", entries.Select(e => $"{Key(e.Key)}: {Format(e.Value)}"))
            .Append('}');
        return EndLine(null);
    }

    public override string ToString() => _text.ToString();

    public static string Format(object? value) => value switch
    {
        null => "null",
        string text => Scalar(text),
        bool flag => flag ? "1" : "0",
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        double number => number.ToString("R", CultureInfo.InvariantCulture),
        float number => number.ToString("R", CultureInfo.InvariantCulture),
        int or long or short or byte => Convert.ToString(value, CultureInfo.InvariantCulture)!,
        DateTime date => Quote(date.TimeOfDay == TimeSpan.Zero
            ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
        IEnumerable items => "[" + string.Join(", ", items.Cast<object?>().Select(Format)) + "]",
        _ => Scalar(Convert.ToString(value, CultureInfo.InvariantCulture) ?? "")
    };

    // Texto sem aspas só quando não pode ser lido como outro tipo nem quebrar a sintaxe; textos com
    // espaço também vão entre aspas, como nas descrições das amostras.
    public static string Scalar(string text) => NeedsQuotes(text) ? Quote(text) : text;

    public static string Quote(string text)
    {
        var quoted = new StringBuilder(text.Length + 2).Append('"');
        foreach (var c in text)
        {
            quoted.Append(c switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ when char.IsControl(c) => $"\\x{(int)c:X2}",
                _ => c.ToString()
            });
        }

        return quoted.Append('"').ToString();
    }

    private static string Key(string key) => NeedsQuotes(key) ? Quote(key) : key;

    private static bool NeedsQuotes(string text) =>
        text.Length == 0 || !PlainScalarRegex().IsMatch(text) || ReservedWordRegex().IsMatch(text) || NumberLikeRegex().IsMatch(text);

    private StringBuilder Indent(int indent) => _text.Append(' ', indent);

    private YamlText EndLine(string? comment)
    {
        if (!string.IsNullOrEmpty(comment))
        {
            _text.Append("   # ").Append(comment);
        }

        _text.Append('\n');
        return this;
    }

    // Letras (inclusive acentuadas), dígitos e . _ / - sem começar por - ou .
    [GeneratedRegex(@"^[\p{L}\p{N}_/][\p{L}\p{N}_./-]*$")]
    private static partial Regex PlainScalarRegex();

    [GeneratedRegex(@"^(null|~|true|false|yes|no|on|off|y|n)$", RegexOptions.IgnoreCase)]
    private static partial Regex ReservedWordRegex();

    [GeneratedRegex(@"^[0-9]+([._][0-9]*)*([eE][0-9]+)?$|^0[xXoObB]|^\d+:\d+")]
    private static partial Regex NumberLikeRegex();
}
