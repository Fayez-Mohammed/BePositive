using Base.DAL.Models.InventoryModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.InventoryConfig
{
    public class BloodInventoryConfiguration : IEntityTypeConfiguration<BloodInventory>
    {
        public void Configure(EntityTypeBuilder<BloodInventory> builder)
        {
            builder.ToTable("BloodInventories");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.HospitalId).IsRequired();
            builder.Property(b => b.BloodTypeId).IsRequired();
            builder.Property(b => b.TotalUnits).IsRequired().HasDefaultValue(0);
            builder.Property(b => b.LastUpdated).IsRequired();

            // ✅ Unique constraint — one record per hospital per blood type
            builder.HasIndex(b => new { b.HospitalId, b.BloodTypeId })
                   .IsUnique();

            // Relationships
            builder.HasOne(b => b.Hospital)
                   .WithMany()
                   .HasForeignKey(b => b.HospitalId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.BloodType)
                   .WithMany()
                   .HasForeignKey(b => b.BloodTypeId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.Batches)
                   .WithOne(b => b.BloodInventory)
                   .HasForeignKey(b => b.BloodInventoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.Transactions)
                   .WithOne(t => t.BloodInventory)
                   .HasForeignKey(t => t.BloodInventoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}