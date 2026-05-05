using Base.DAL.Models.DonorModels;
using Base.DAL.Models.MessagingModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Base.DAL.Config.ConversationConfig
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.HasIndex(c => new { c.HospitalId, c.DonorId })
      .IsUnique();



        }
    }
}
