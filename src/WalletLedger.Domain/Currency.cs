namespace WalletLedger.Domain;

public sealed record Currency
{
    public static readonly Currency Azn = new("AZN", 2);
    public static readonly Currency Usd = new("USD", 2);
    public static readonly Currency Eur = new("EUR", 2);

    private static readonly IReadOnlyDictionary<string, Currency> Supported =
        new[] { Azn, Usd, Eur }.ToDictionary(currency => currency.Code);

    private Currency(string code, int decimalPlaces)
    {
        Code = code;
        DecimalPlaces = decimalPlaces;
        MinorUnitFactor = Pow10(decimalPlaces);
    }

    public string Code { get; }
    public int DecimalPlaces { get; }
    public long MinorUnitFactor { get; }

    public static Currency Parse(string? code)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        return Supported.TryGetValue(normalized, out var currency)
            ? currency
            : throw new UnsupportedCurrencyException(code);
    }

    public override string ToString() => Code;

    private static long Pow10(int exponent)
    {
        var result = 1L;
        for (var i = 0; i < exponent; i++)
        {
            result *= 10;
        }

        return result;
    }
}