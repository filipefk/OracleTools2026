using System.Buffers.Binary;
using System.Text;

namespace OracleTools2026.Data.Senior;

// Decodifica o fonte LSP gravado em R960RUL.RULDAT. Formato observado nas regras do HML:
// bytes 0-3 = tamanho total - 4, 4-11 = fixos, 12 = chave XOR, 13-16 = tamanho do corpo e, a partir do 17,
// o corpo (texto ISO-8859-1 com XOR da chave). A chave varia por regra (0x0A e 0xAB nas amostras).
public sealed class SeniorRuleDecoder
{
    private const int HeaderLength = 17;
    private const int KeyOffset = 12;
    private const int BodyLengthOffset = 13;

    public SeniorRuleDecodeResult Decode(byte[]? data)
    {
        if (data is null || data.Length == 0)
        {
            return SeniorRuleDecodeResult.Failure("regra vazia");
        }

        if (data.Length < HeaderLength)
        {
            return SeniorRuleDecodeResult.Failure($"conteúdo com {data.Length} bytes, menor que o cabeçalho");
        }

        var totalLength = data.Length - 4;
        var bodyLength = data.Length - HeaderLength;
        if (!MatchesLength(data.AsSpan(0, 4), totalLength) || !MatchesLength(data.AsSpan(BodyLengthOffset, 4), bodyLength))
        {
            return SeniorRuleDecodeResult.Failure(
                $"cabeçalho em formato desconhecido (tamanho {Convert.ToHexString(data, 0, 4)}, corpo " +
                $"{Convert.ToHexString(data, BodyLengthOffset, 4)}, esperado {totalLength} e {bodyLength} bytes)");
        }

        var key = data[KeyOffset];
        var body = new byte[bodyLength];
        for (var i = 0; i < bodyLength; i++)
        {
            body[i] = (byte)(data[HeaderLength + i] ^ key);
        }

        var source = Encoding.Latin1.GetString(body).Replace("\r\n", "\n");
        return new SeniorRuleDecodeResult(source, null);
    }

    // A ordem dos bytes não foi confirmada; aceita as duas.
    private static bool MatchesLength(ReadOnlySpan<byte> bytes, int expected) =>
        BinaryPrimitives.ReadInt32LittleEndian(bytes) == expected || BinaryPrimitives.ReadInt32BigEndian(bytes) == expected;
}
