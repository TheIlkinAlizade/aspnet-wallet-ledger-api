namespace WalletLedger.Domain;

public class DomainException(string message) : Exception(message);

public sealed class CurrencyMismatchException(Currency expected, Currency actual)
    : DomainException($"Currency mismatch: expected {expected.Code}, got {actual.Code}.");

public sealed class UnsupportedCurrencyException(string? code)
    : DomainException($"Currency '{code}' is not supported.");

public sealed class AmountOutOfRangeException(string value)
    : DomainException($"Amount '{value}' is outside the allowed range.");

public sealed class InvalidAmountException(string message) : DomainException(message);

public sealed class InsufficientFundsException(Guid accountId)
    : DomainException($"Account {accountId} has insufficient funds.");

public sealed class InvalidAccountException(string message) : DomainException(message);

public sealed class InvalidLedgerTransactionException(string message) : DomainException(message);

public sealed class InvalidTransferException(string message) : DomainException(message);