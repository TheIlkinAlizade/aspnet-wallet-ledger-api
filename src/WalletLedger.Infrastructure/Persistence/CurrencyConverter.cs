using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WalletLedger.Domain;

namespace WalletLedger.Infrastructure.Persistence;

public sealed class CurrencyConverter()
    : ValueConverter<Currency, string>(currency => currency.Code, code => Currency.Parse(code));