using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletLedger.Domain;

namespace WalletLedger.Infrastructure.Persistence.Configurations;

public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers", table =>
        {
            table.HasCheckConstraint("CK_Transfers_PositiveAmount", "\"AmountMinorUnits\" > 0");
            table.HasCheckConstraint("CK_Transfers_DifferentAccounts", "\"SourceAccountId\" <> \"DestinationAccountId\"");
        });

        builder.HasKey(transfer => transfer.Id);
        builder.Property(transfer => transfer.Id).ValueGeneratedNever();
        builder.Property(transfer => transfer.Status).HasConversion<string>().HasMaxLength(32);

        builder.Ignore(transfer => transfer.Amount);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(transfer => transfer.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(transfer => transfer.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LedgerTransaction>()
            .WithOne()
            .HasForeignKey<Transfer>(transfer => transfer.LedgerTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transfer => new { transfer.SourceAccountId, transfer.CreatedAt });
        builder.HasIndex(transfer => new { transfer.DestinationAccountId, transfer.CreatedAt });
    }
}