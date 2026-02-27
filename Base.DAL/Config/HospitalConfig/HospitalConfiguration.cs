using Base.DAL.Models.HospitalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.HospitalConfig
{
    public class HospitalConfiguration : IEntityTypeConfiguration<Hospital>
    {
        public void Configure(EntityTypeBuilder<Hospital> builder)
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Name).HasMaxLength(150).IsRequired();
            builder.Property(h => h.LicenseNumber).HasMaxLength(50);
            builder.HasIndex(h => h.LicenseNumber).IsUnique().HasFilter("[LicenseNumber] IS NOT NULL");
            builder.Property(h => h.Email).HasMaxLength(150);
            builder.Property(h => h.Phone).HasMaxLength(20);
            builder.Property(h => h.Status).HasMaxLength(20).HasDefaultValue("UnderReview");
            builder.Property(h => h.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(h => h.Longitude).HasColumnType("decimal(9,6)");

            builder.HasOne(h => h.Governorate)
                   .WithMany(g => g.Hospitals)
                   .HasForeignKey(h => h.GovernorateId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(h => h.City)
                   .WithMany(c => c.Hospitals)
                   .HasForeignKey(h => h.CityId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
