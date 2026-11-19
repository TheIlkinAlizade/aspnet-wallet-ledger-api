using System.Globalization;

namespace WalletLedger.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var result = Money.Of(150, Currency.Usd).Add(Money.Of(250, Currency.Usd));

        Assert.Equal(Money.Of(400, Currency.Usd), result);
    }

    [Fact]
    public void Add_DifferentCurrencies_Throws()
    {
        Assert.Throws<CurrencyMismatchException>(
            () => Money.Of(1, Currency.Usd).Add(Money.Of(1, Currency.Eur)));
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var result = Money.Of(500, Currency.Azn).Subtract(Money.Of(200, Currency.Azn));

        Assert.Equal(Money.Of(300, Currency.Azn), result);
    }

    [Fact]
    public void Add_ResultAboveMaximum_Throws()
    {
        var max = Money.Of(Money.MaxMinorUnits, Currency.Usd);

        Assert.Throws<AmountOutOfRangeException>(() => max.Add(Money.Of(1, Currency.Usd)));
    }

    [Fact]
    public void Of_AboveMaximum_Throws()
    {
        Assert.Throws<AmountOutOfRangeException>(() => Money.Of(Money.MaxMinorUnits + 1, Currency.Usd));
    }

    [Fact]
    public void Of_BelowNegativeMaximum_Throws()
    {
        Assert.Throws<AmountOutOfRangeException>(() => Money.Of(-Money.MaxMinorUnits - 1, Currency.Usd));
    }

    [Fact]
    public void Negate_FlipsSign()
    {
        Assert.Equal(Money.Of(-700, Currency.Usd), Money.Of(700, Currency.Usd).Negate());
    }

    [Fact]
    public void Negate_Twice_ReturnsOriginal()
    {
        var original = Money.Of(700, Currency.Usd);

        Assert.Equal(original, original.Negate().Negate());
    }

    [Fact]
    public void SignChecks_ReflectAmount()
    {
        Assert.True(Money.Zero(Currency.Usd).IsZero);
        Assert.True(Money.Of(1, Currency.Usd).IsPositive);
        Assert.True(Money.Of(-1, Currency.Usd).IsNegative);
    }

    [Fact]
    public void Comparison_DifferentCurrencies_Throws()
    {
        Assert.Throws<CurrencyMismatchException>(
            () => Money.Of(1, Currency.Usd) < Money.Of(2, Currency.Eur));
    }

    [Theory]
    [InlineData("10.50", 1050)]
    [InlineData("10.5", 1050)]
    [InlineData("10", 1000)]
    [InlineData("0.01", 1)]
    [InlineData("-5.25", -525)]
    public void Parse_ValidText_ReturnsMinorUnits(string text, long expected)
    {
        Assert.Equal(expected, Money.Parse(text, Currency.Usd).MinorUnits);
    }

    [Theory]
    [InlineData("10.005")]
    [InlineData("0.001")]
    [InlineData("1.999")]
    public void Parse_TooManyDecimals_Throws(string text)
    {
        Assert.Throws<InvalidAmountException>(() => Money.Parse(text, Currency.Usd));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("1,5")]
    [InlineData("1e3")]
    [InlineData(" 5")]
    public void Parse_NotANumber_Throws(string text)
    {
        Assert.Throws<InvalidAmountException>(() => Money.Parse(text, Currency.Usd));
    }

    [Theory]
    [InlineData("9007199254740991")]
    [InlineData("9007199254740992")]
    [InlineData("79228162514264337593543950335")]
    public void Parse_AboveMaximum_Throws(string text)
    {
        Assert.Throws<AmountOutOfRangeException>(() => Money.Parse(text, Currency.Usd));
    }

    [Theory]
    [InlineData(1, "0.5", 0)]
    [InlineData(3, "0.5", 2)]
    [InlineData(5, "0.5", 2)]
    [InlineData(15, "0.5", 8)]
    public void Multiply_RoundsHalfToEven(long minorUnits, string factor, long expected)
    {
        var result = Money.Of(minorUnits, Currency.Usd)
            .Multiply(decimal.Parse(factor, CultureInfo.InvariantCulture));

        Assert.Equal(expected, result.MinorUnits);
    }

    [Fact]
    public void Multiply_ResultAboveMaximum_Throws()
    {
        Assert.Throws<AmountOutOfRangeException>(
            () => Money.Of(Money.MaxMinorUnits, Currency.Usd).Multiply(2m));
    }

    [Fact]
    public void Multiply_DecimalOverflow_ThrowsDomainException()
    {
        Assert.Throws<AmountOutOfRangeException>(
            () => Money.Of(2, Currency.Usd).Multiply(decimal.MaxValue));
    }

    [Fact]
    public void ToString_FormatsWithCurrencyDecimals()
    {
        Assert.Equal("10.05 USD", Money.Of(1005, Currency.Usd).ToString());
    }

    [Theory]
    [InlineData("usd")]
    [InlineData(" USD ")]
    public void CurrencyParse_IsCaseAndWhitespaceInsensitive(string code)
    {
        Assert.Equal(Currency.Usd, Currency.Parse(code));
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("")]
    [InlineData(null)]
    public void CurrencyParse_UnknownCode_Throws(string? code)
    {
        Assert.Throws<UnsupportedCurrencyException>(() => Currency.Parse(code));
    }
}