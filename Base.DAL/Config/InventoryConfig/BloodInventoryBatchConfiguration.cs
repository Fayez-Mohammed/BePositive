using Base.DAL.Models.InventoryModels;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.InventoryConfig
{
    public class BloodInventoryBatchConfiguration : IEntityTypeConfiguration<BloodInventoryBatch>
    {
        public void Configure(EntityTypeBuilder<BloodInventoryBatch> builder)
        {
            builder.ToTable("BloodInventoryBatches");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.HospitalId).IsRequired();
            builder.Property(b => b.BloodTypeId).IsRequired();
            builder.Property(b => b.BloodInventoryId).IsRequired();  // ← not nullable
            builder.Property(b => b.Units).IsRequired();
            builder.Property(b => b.RemainingUnits).IsRequired();
            builder.Property(b => b.CollectionDate).IsRequired();
            builder.Property(b => b.ExpiryDate).IsRequired();
            builder.Property(b => b.Status)
                   .IsRequired()
                   .HasDefaultValue(BatchStatus.Active);

            builder.HasOne(b => b.Hospital)
                   .WithMany()
                   .HasForeignKey(b => b.HospitalId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.BloodType)
                   .WithMany()
                   .HasForeignKey(b => b.BloodTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}