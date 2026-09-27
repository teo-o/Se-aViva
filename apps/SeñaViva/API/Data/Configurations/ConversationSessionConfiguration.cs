using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenaViva.Shared.Entities;

namespace SenaViva.API.Data.Configurations;

public class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.Property(session => session.Status).HasConversion<string>().HasMaxLength(60);
        builder.Property(session => session.SignLanguage).HasConversion<string>().HasMaxLength(10);

        builder.HasOne(session => session.User)
            .WithMany(user => user.ConversationSessions)
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
