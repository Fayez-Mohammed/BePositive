using Base.DAL.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.RequestConfig
{
    public class DonationHistoryConfiguration : IEntityTypeConfiguration<DonationHistory>
    {
        public void Configure(EntityTypeBuilder<DonationHistory> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.DonationDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(d => d.Donor)
                   .WithMany(donor => donor.DonationHistories)
                   .HasForeignKey(d => d.DonorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Hospital)
                   .WithMany(h => h.DonationHistories)
                   .HasForeignKey(d => d.HospitalId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(d => d.Request)
                   .WithMany(r => r.DonationHistories)
                   .HasForeignKey(d => d.RequestId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
