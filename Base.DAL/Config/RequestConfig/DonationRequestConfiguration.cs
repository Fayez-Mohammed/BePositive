using Base.DAL.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.RequestConfig
{
    public class DonationRequestConfiguration : IEntityTypeConfiguration<DonationRequest>
    {
        public void Configure(EntityTypeBuilder<DonationRequest> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.UrgencyLevel).HasMaxLength(20).IsRequired();
            builder.Property(r => r.Note).HasMaxLength(255);
            builder.Property(r => r.Status).HasMaxLength(20).HasDefaultValue("Open");
            builder.Property(r => r.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(r => r.Longitude).HasColumnType("decimal(9,6)");

            builder.HasOne(r => r.Hospital)
                   .WithMany(h => h.DonationRequests)
                   .HasForeignKey(r => r.HospitalId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.BloodType)
                   .WithMany(b => b.DonationRequests)
                   .HasForeignKey(r => r.BloodTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
