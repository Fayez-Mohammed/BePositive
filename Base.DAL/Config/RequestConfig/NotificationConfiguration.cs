using Base.DAL.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.RequestConfig
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).HasMaxLength(100);
            builder.Property(n => n.Body).HasMaxLength(255);
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(n => n.User)
                   .WithMany()
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.RelatedRequest)
                   .WithMany(r => r.Notifications)
                   .HasForeignKey(n => n.RelatedRequestId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
