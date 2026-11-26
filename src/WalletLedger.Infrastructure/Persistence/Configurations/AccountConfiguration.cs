using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletLedger.Domain;
using WalletLedger.Infrastructure.Identity;

namespace WalletLedger.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts", table => table.HasCheckConstraint(
            "CK_Accounts_NonNegativeBalance",
            "\"BalanceMinorUnits\" >= 0 OR \"Kind\" = 'SystemClearing'"));

        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).ValueGeneratedNever();
        builder.Property(account => account.Kind).HasConversion<string>().HasMaxLength(32);

        builder.Ignore(account => account.Balance);
        builder.Ignore(account => account.AllowsNegativeBalance);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(account => account.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(account => account.OwnerUserId);
        builder.HasIndex(account => new { account.Currency, account.Kind })
            .HasFilter("\"Kind\" <> 'User'")
            .IsUnique();
        builder.HasData(SystemAccountSeed.Rows());
    }
}