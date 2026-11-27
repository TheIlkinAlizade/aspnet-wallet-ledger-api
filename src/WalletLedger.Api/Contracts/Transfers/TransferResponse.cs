using WalletLedger.Application.Transfers;

namespace WalletLedger.Api.Contracts.Transfers;

public sealed record TransferResponse(
    Guid Id,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string Currency,
    long AmountMinorUnits,
    string Amount,
    string Status,
    DateTimeOffset CreatedAt)
{
    public static TransferResponse From(TransferView view) => new(
        view.Id,
        view.SourceAccountId,
        view.DestinationAccountId,
        view.Amount.Currency.Code,
        view.Amount.MinorUnits,
        view.Amount.ToAmountString(),
        view.Status.ToString(),
        view.CreatedAt);
}