using System.Globalization;

namespace WalletLedger.Domain;

public sealed record Money : IComparable<Money>
{
    public const long MaxMinorUnits = 9_007_199_254_740_991L;

    private Money(long minorUnits, Currency currency)
    {
        MinorUnits = minorUnits;
        Currency = currency;
    }

    public long MinorUnits { get; }
    public Currency Currency { get; }
    public bool IsZero => MinorUnits == 0;
    public bool IsPositive => MinorUnits > 0;
    public bool IsNegative => MinorUnits < 0;

    public static Money Of(long minorUnits, Currency currency)
    {
        if (minorUnits > MaxMinorUnits || minorUnits < -MaxMinorUnits)
        {
            throw new AmountOutOfRangeException(minorUnits.ToString(CultureInfo.InvariantCulture));
        }

        return new Money(minorUnits, currency);
    }

    public static Money Zero(Currency currency) => Of(0, currency);

    public static Money Parse(string text, Currency currency)
    {
        var amount = ParseDecimal(text);

        if (Math.Abs(amount) > MaxMinorUnits)
        {
            throw new AmountOutOfRangeException(text);
        }

        var minorUnits = amount * currency.MinorUnitFactor;

        if (minorUnits != decimal.Truncate(minorUnits))
        {
            throw new InvalidAmountException(
                $"{currency.Code} allows at most {currency.DecimalPlaces} decimal places.");
        }

        return Of(decimal.ToInt64(minorUnits), currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Of(checked(MinorUnits + other.MinorUnits), Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Of(checked(MinorUnits - other.MinorUnits), Currency);
    }

    public Money Negate() => Of(-MinorUnits, Currency);

    public Money Multiply(decimal factor)
    {
        try
        {
            var rounded = Math.Round(MinorUnits * factor, 0, MidpointRounding.ToEven);
            return Of(decimal.ToInt64(rounded), Currency);
        }
        catch (OverflowException)
        {
            throw new AmountOutOfRangeException($"{MinorUnits} x {factor}");
        }
    }

    public decimal ToDecimal() => (decimal)MinorUnits / Currency.MinorUnitFactor;

    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(other);
        return MinorUnits.CompareTo(other.MinorUnits);
    }

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public override string ToString() =>
        $"{ToDecimal().ToString($"F{Currency.DecimalPlaces}", CultureInfo.InvariantCulture)} {Currency.Code}";

    private static decimal ParseDecimal(string text)
    {
        const NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

        if (!decimal.TryParse(text, styles, CultureInfo.InvariantCulture, out var amount))
        {
            throw new InvalidAmountException($"'{text}' is not a valid amount.");
        }

        return amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new CurrencyMismatchException(Currency, other.Currency);
        }
    }
}