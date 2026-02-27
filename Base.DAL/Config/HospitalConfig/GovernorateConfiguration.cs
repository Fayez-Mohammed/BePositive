using Base.DAL.Models.HospitalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.HospitalConfig
{
    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.HasKey(g => g.Id);
            builder.Property(g => g.NameAr).HasMaxLength(50).IsRequired();
            builder.Property(g => g.NameEn).HasMaxLength(50).IsRequired();
        }
    }
}
