using Base.DAL.Models.HospitalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.HospitalConfig
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.NameAr).HasMaxLength(50).IsRequired();
            builder.Property(c => c.NameEn).HasMaxLength(50).IsRequired();

            builder.HasOne(c => c.Governorate)
                   .WithMany(g => g.Cities)
                   .HasForeignKey(c => c.GovernorateId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
