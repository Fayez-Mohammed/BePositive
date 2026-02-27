using Base.DAL.Models.DonorModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.DonorConfig
{
    public class DonorConfiguration : IEntityTypeConfiguration<Donor>
    {
        public void Configure(EntityTypeBuilder<Donor> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.NationalId).HasMaxLength(20);
            builder.HasIndex(d => d.NationalId).IsUnique().HasFilter("[NationalId] IS NOT NULL");

            builder.Property(d => d.Gender).HasMaxLength(10);
            builder.Property(d => d.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(d => d.Longitude).HasColumnType("decimal(9,6)");

            builder.HasOne(d => d.User)
                   .WithOne()
                   .HasForeignKey<Donor>(d => d.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.BloodType)
                   .WithMany(b => b.Donors)
                   .HasForeignKey(d => d.BloodTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.City)
                   .WithMany(c => c.Donors)
                   .HasForeignKey(d => d.CityId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
