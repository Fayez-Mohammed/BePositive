using Base.DAL.Models.BloodModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.BloodConfig
{
    public class BloodTypeCompatibilityConfiguration : IEntityTypeConfiguration<BloodTypeCompatibility>
    {
        public void Configure(EntityTypeBuilder<BloodTypeCompatibility> builder)
        {
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.DonorBloodType)
                   .WithMany(bt => bt.CanDonateTo)
                   .HasForeignKey(b => b.DonorBloodTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.RecipientBloodType)
                   .WithMany(bt => bt.CanReceiveFrom)
                   .HasForeignKey(b => b.RecipientBloodTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Seed data using the fixed IDs from BloodTypeConfiguration
            // bt-oneg=O-, bt-opos=O+, bt-aneg=A-, bt-apos=A+
            // bt-bneg=B-, bt-bpos=B+, bt-abneg=AB-, bt-abpos=AB+
            builder.HasData(
                // O- donates to all 8
                new BloodTypeCompatibility { Id = "bc-01", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-apos"  },
                new BloodTypeCompatibility { Id = "bc-02", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-aneg"  },
                new BloodTypeCompatibility { Id = "bc-03", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-bpos"  },
                new BloodTypeCompatibility { Id = "bc-04", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-bneg"  },
                new BloodTypeCompatibility { Id = "bc-05", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-abpos" },
                new BloodTypeCompatibility { Id = "bc-06", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-abneg" },
                new BloodTypeCompatibility { Id = "bc-07", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-opos"  },
                new BloodTypeCompatibility { Id = "bc-08", DonorBloodTypeId = "bt-oneg", RecipientBloodTypeId = "bt-oneg"  },
                // O+ -> O+, A+, B+, AB+
                new BloodTypeCompatibility { Id = "bc-09", DonorBloodTypeId = "bt-opos", RecipientBloodTypeId = "bt-opos"  },
                new BloodTypeCompatibility { Id = "bc-10", DonorBloodTypeId = "bt-opos", RecipientBloodTypeId = "bt-apos"  },
                new BloodTypeCompatibility { Id = "bc-11", DonorBloodTypeId = "bt-opos", RecipientBloodTypeId = "bt-bpos"  },
                new BloodTypeCompatibility { Id = "bc-12", DonorBloodTypeId = "bt-opos", RecipientBloodTypeId = "bt-abpos" },
                // A- -> A-, A+, AB-, AB+
                new BloodTypeCompatibility { Id = "bc-13", DonorBloodTypeId = "bt-aneg", RecipientBloodTypeId = "bt-aneg"  },
                new BloodTypeCompatibility { Id = "bc-14", DonorBloodTypeId = "bt-aneg", RecipientBloodTypeId = "bt-apos"  },
                new BloodTypeCompatibility { Id = "bc-15", DonorBloodTypeId = "bt-aneg", RecipientBloodTypeId = "bt-abneg" },
                new BloodTypeCompatibility { Id = "bc-16", DonorBloodTypeId = "bt-aneg", RecipientBloodTypeId = "bt-abpos" },
                // A+ -> A+, AB+
                new BloodTypeCompatibility { Id = "bc-17", DonorBloodTypeId = "bt-apos", RecipientBloodTypeId = "bt-apos"  },
                new BloodTypeCompatibility { Id = "bc-18", DonorBloodTypeId = "bt-apos", RecipientBloodTypeId = "bt-abpos" },
                // B- -> B-, B+, AB-, AB+
                new BloodTypeCompatibility { Id = "bc-19", DonorBloodTypeId = "bt-bneg", RecipientBloodTypeId = "bt-bneg"  },
                new BloodTypeCompatibility { Id = "bc-20", DonorBloodTypeId = "bt-bneg", RecipientBloodTypeId = "bt-bpos"  },
                new BloodTypeCompatibility { Id = "bc-21", DonorBloodTypeId = "bt-bneg", RecipientBloodTypeId = "bt-abneg" },
                new BloodTypeCompatibility { Id = "bc-22", DonorBloodTypeId = "bt-bneg", RecipientBloodTypeId = "bt-abpos" },
                // B+ -> B+, AB+
                new BloodTypeCompatibility { Id = "bc-23", DonorBloodTypeId = "bt-bpos", RecipientBloodTypeId = "bt-bpos"  },
                new BloodTypeCompatibility { Id = "bc-24", DonorBloodTypeId = "bt-bpos", RecipientBloodTypeId = "bt-abpos" },
                // AB- -> AB-, AB+
                new BloodTypeCompatibility { Id = "bc-25", DonorBloodTypeId = "bt-abneg", RecipientBloodTypeId = "bt-abneg" },
                new BloodTypeCompatibility { Id = "bc-26", DonorBloodTypeId = "bt-abneg", RecipientBloodTypeId = "bt-abpos" },
                // AB+ -> AB+ only
                new BloodTypeCompatibility { Id = "bc-27", DonorBloodTypeId = "bt-abpos", RecipientBloodTypeId = "bt-abpos" }
            );
        }
    }
}
