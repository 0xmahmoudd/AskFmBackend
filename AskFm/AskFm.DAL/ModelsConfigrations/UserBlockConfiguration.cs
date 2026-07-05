using AskFm.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AskFm.DAL.ModelsConfigrations;

public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.HasKey(ub => ub.Id);

        builder.HasIndex(ub => new { ub.BlockerId, ub.BlockedId }).IsUnique();

        builder.HasOne(ub => ub.Blocker)
            .WithMany(u => u.BlockedUsers)
            .HasForeignKey(ub => ub.BlockerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ub => ub.Blocked)
            .WithMany(u => u.BlockedByUsers)
            .HasForeignKey(ub => ub.BlockedId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
