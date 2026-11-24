using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletLedger.Application.Auth;

namespace WalletLedger.Infrastructure.Persistence.Configurations;

public sealed class RoleSeedConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    private static readonly Guid UserRoleId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminRoleId = new("00000000-0000-0000-0000-000000000002");

    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder) =>
        builder.HasData(
            CreateRole(UserRoleId, Roles.User),
            CreateRole(AdminRoleId, Roles.Admin));

    private static IdentityRole<Guid> CreateRole(Guid id, string name) => new()
    {
        Id = id,
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
        ConcurrencyStamp = id.ToString()
    };
}