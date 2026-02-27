using Base.DAL.Models.BloodModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.BloodConfig
{
    public class BloodTypeConfiguration : IEntityTypeConfiguration<BloodType>
    {
        public void Configure(EntityTypeBuilder<BloodType> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.TypeName).HasMaxLength(5).IsRequired();
            builder.HasIndex(b => b.TypeName).IsUnique();

            // Seed all 8 blood types with fixed GUIDs
            builder.HasData(
                new BloodType { Id = "bt-apos",  TypeName = "A+"  },
                new BloodType { Id = "bt-aneg",  TypeName = "A-"  },
                new BloodType { Id = "bt-bpos",  TypeName = "B+"  },
                new BloodType { Id = "bt-bneg",  TypeName = "B-"  },
                new BloodType { Id = "bt-abpos", TypeName = "AB+" },
                new BloodType { Id = "bt-abneg", TypeName = "AB-" },
                new BloodType { Id = "bt-opos",  TypeName = "O+"  },
                new BloodType { Id = "bt-oneg",  TypeName = "O-"  }
            );
        }
    }
}
