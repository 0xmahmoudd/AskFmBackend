using AskFm.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AskFm.DAL.ModelsConfigrations;

public class UserMuteConfiguration : IEntityTypeConfiguration<UserMute>
{
    public void Configure(EntityTypeBuilder<UserMute> builder)
    {
        builder.HasKey(um => um.Id);

        builder.HasIndex(um => new { um.MuterId, um.MutedId }).IsUnique();

        builder.HasOne(um => um.Muter)
            .WithMany(u => u.MutedUsers)
            .HasForeignKey(um => um.MuterId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(um => um.Muted)
            .WithMany(u => u.MutedByUsers)
            .HasForeignKey(um => um.MutedId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
