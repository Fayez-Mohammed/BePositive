using Base.DAL.Models.HospitalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.HospitalConfig
{
    public class HospitalAdminConfiguration : IEntityTypeConfiguration<HospitalAdmin>
    {
        public void Configure(EntityTypeBuilder<HospitalAdmin> builder)
        {
            builder.HasKey(ha => ha.Id);
            builder.Property(ha => ha.JobTitle).HasMaxLength(50);

            builder.HasOne(ha => ha.User)
                   .WithOne()
                   .HasForeignKey<HospitalAdmin>(ha => ha.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ha => ha.Hospital)
                   .WithOne(h => h.Admin)
                   .HasForeignKey<HospitalAdmin>(ha => ha.HospitalId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
