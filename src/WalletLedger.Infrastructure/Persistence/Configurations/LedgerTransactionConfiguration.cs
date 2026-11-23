using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletLedger.Domain;

namespace WalletLedger.Infrastructure.Persistence.Configurations;

public sealed class LedgerTransactionConfiguration : IEntityTypeConfiguration<LedgerTransaction>
{
    public void Configure(EntityTypeBuilder<LedgerTransaction> builder)
    {
        builder.ToTable("LedgerTransactions");

        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.Id).ValueGeneratedNever();
        builder.Property(transaction => transaction.Type).HasConversion<string>().HasMaxLength(32);

        builder.HasOne<LedgerTransaction>()
            .WithMany()
            .HasForeignKey(transaction => transaction.ReversalOfId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(transaction => transaction.ReversalOfId).IsUnique();

        builder.HasMany(transaction => transaction.Entries)
            .WithOne()
            .HasForeignKey(entry => entry.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(transaction => transaction.Entries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}