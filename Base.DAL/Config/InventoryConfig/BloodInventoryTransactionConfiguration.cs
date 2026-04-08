using Base.DAL.Models.InventoryModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.InventoryConfig
{
    public class BloodInventoryTransactionConfiguration : IEntityTypeConfiguration<BloodInventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<BloodInventoryTransaction> builder)
        {
            builder.ToTable("BloodInventoryTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.HospitalId).IsRequired();
            builder.Property(t => t.BloodTypeId).IsRequired();
            builder.Property(t => t.BloodInventoryId).IsRequired();  // ← not nullable
            builder.Property(t => t.ChangeAmount).IsRequired();
            builder.Property(t => t.Reason).IsRequired();
            builder.Property(t => t.ChangedById).IsRequired();
            builder.Property(t => t.ChangedAt).IsRequired();
            builder.Property(t => t.Notes).HasMaxLength(500);

            builder.HasOne(t => t.Hospital)
                   .WithMany()
                   .HasForeignKey(t => t.HospitalId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.BloodType)
                   .WithMany()
                   .HasForeignKey(t => t.BloodTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}