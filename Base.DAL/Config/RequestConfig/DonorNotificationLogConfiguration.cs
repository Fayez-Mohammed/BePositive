using Base.DAL.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.RequestConfig
{
    public class DonorNotificationLogConfiguration : IEntityTypeConfiguration<DonorNotificationLog>
    {
        public void Configure(EntityTypeBuilder<DonorNotificationLog> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Channel).HasMaxLength(20);
            builder.Property(n => n.SentAt).HasDefaultValueSql("GETUTCDATE()");

            // Composite index to prevent duplicate notifications
            builder.HasIndex(n => new { n.DonorId, n.RequestId });

            builder.HasOne(n => n.Donor)
                   .WithMany(d => d.NotificationLogs)
                   .HasForeignKey(n => n.DonorId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.Request)
                   .WithMany(r => r.NotificationLogs)
                   .HasForeignKey(n => n.RequestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
