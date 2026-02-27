using Base.DAL.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.RequestConfig
{
    public class RequestResponseConfiguration : IEntityTypeConfiguration<RequestResponse>
    {
        public void Configure(EntityTypeBuilder<RequestResponse> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Status).HasMaxLength(20).HasDefaultValue("Pending");
            builder.Property(r => r.DonorDistanceKm).HasColumnType("decimal(5,2)");
            builder.Property(r => r.RespondedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(r => r.Request)
                   .WithMany(dr => dr.Responses)
                   .HasForeignKey(r => r.RequestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Donor)
                   .WithMany(d => d.RequestResponses)
                   .HasForeignKey(r => r.DonorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
