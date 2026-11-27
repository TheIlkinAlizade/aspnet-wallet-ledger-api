namespace WalletLedger.Application.Transfers;

public interface ITransferService
{
    Task<TransferView> TransferAsync(
        Guid userId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        string amount,
        CancellationToken cancellationToken);
}