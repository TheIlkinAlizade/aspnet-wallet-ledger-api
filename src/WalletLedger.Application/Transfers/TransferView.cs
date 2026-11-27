using WalletLedger.Domain;

namespace WalletLedger.Application.Transfers;

public sealed record TransferView(
    Guid Id,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    Money Amount,
    TransferStatus Status,
    DateTimeOffset CreatedAt)
{
    public static TransferView From(Transfer transfer) => new(
        transfer.Id,
        transfer.SourceAccountId,
        transfer.DestinationAccountId,
        transfer.Amount,
        transfer.Status,
        transfer.CreatedAt);
}