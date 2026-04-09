using Base.DAL.Models.BaseModels;
using Base.DAL.Models.BloodModels;
using Base.DAL.Models.DonorModels;
using Base.DAL.Models.HospitalModels;
using Base.DAL.Models.InventoryModels;
using Base.DAL.Models.RequestModels;
using Base.DAL.Models.SystemModels;
using Base.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using BloodManagement.Gamification.Domain;
using CommunityApp.Models;
using BaseEntity = Base.DAL.Models.BaseModels.BaseEntity;

namespace Base.DAL.Contexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public AppDbContext(DbContextOptions<AppDbContext> options,
                            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Global query filters — automatically exclude soft-deleted records
            builder.Entity<Donor>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Hospital>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<HospitalAdmin>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<DonationRequest>().HasQueryFilter(e => !e.IsDeleted);
                builder.Entity<UserGamificationProfile>(entity =>
{
    entity.HasKey(e => e.UserId);
    // Configure Value Object Points
    entity.OwnsOne(e => e.TotalPoints, points =>
    {
        points.Property(p => p.Value).HasColumnName("TotalPoints");
    });
    // Configure TierLevel enum to be stored as string
    entity.Property(e => e.CurrentTier)
          .HasConversion<string>();

    // Configure Achievements owned by UserGamificationProfile
    entity.OwnsMany(e => e.Achievements, achievement =>
    {
        achievement.WithOwner().HasForeignKey("UserId");
        achievement.HasKey("Id"); // Shadow property for primary key
        achievement.Property(a => a.Status)
                   .HasConversion<string>();
        achievement.Property(a => a.AchievementId).IsRequired();
        achievement.Property(a => a.CurrentProgress).IsRequired();
        achievement.Property(a => a.UnlockedDate);
    });

    // Configure RedeemedRewards owned by UserGamificationProfile
    entity.OwnsMany(e => e.RedeemedRewards, reward =>
    {
        reward.WithOwner().HasForeignKey("UserId");
        reward.HasKey("Id"); // Shadow property for primary key
        reward.Property(r => r.RewardId).IsRequired();
        reward.Property(r => r.RedeemedDate).IsRequired();
    });
});

// Configure Achievement entity
builder.Entity<Achievement>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Type)
          .HasConversion<string>();
});

// Configure Reward entity
builder.Entity<Reward>(entity =>
{
    entity.HasKey(e => e.Id);
    // Important: Define the owned type property mapping
    entity.OwnsOne(e => e.Cost, cost =>
    {
        cost.Property(p => p.Value).HasColumnName("Cost");
    });
    entity.Property(e => e.Type)
          .HasConversion<string>();
});

// Seed initial data for Achievements
builder.Entity<Achievement>().HasData(
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "First Drop", Description = "Complete your first blood donation", PointsAwarded = 100, Icon = "heart", Type = AchievementType.SingleEvent, TargetValue = 1 },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Life Saver", Description = "Save 3 lives through donations", PointsAwarded = 250, Icon = "star", Type = AchievementType.Cumulative, TargetValue = 3 },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "Gold Donor", Description = "Reach Gold tier status", PointsAwarded = 500, Icon = "trophy", Type = AchievementType.TierBased, TargetValue = (int)TierLevel.Gold },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = "Speed Demon", Description = "Donate within 5 days of request", PointsAwarded = 150, Icon = "lightning", Type = AchievementType.TimeBased, TargetValue = 5 },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = "Consistency Champion", Description = "Donate 3 times in a year", PointsAwarded = 300, Icon = "lock", Type = AchievementType.Cumulative, TargetValue = 3 },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), Name = "Platinum Hero", Description = "Reach Platinum tier status", PointsAwarded = 1000, Icon = "lock", Type = AchievementType.TierBased, TargetValue = (int)TierLevel.Platinum }
);

// Seed initial data for Rewards using Anonymous Types to handle Owned Types (Cost)
// Note: EF Core requires seeding owned properties using their shadow property names (RewardId + PropertyName)
builder.Entity<Reward>().HasData(
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), Name = "Free Health Checkup", Description = "Comprehensive health screening", Image = "health_checkup_icon", Type = RewardType.Service },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000008"), Name = "Be Positive T-Shirt", Description = "Limited edition donor merch", Image = "tshirt_icon", Type = RewardType.PhysicalItem },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000009"), Name = "Priority Scheduling", Description = "Skip the wait for 6 months", Image = "alarm_icon", Type = RewardType.Service },
    new { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), Name = "Coffee Voucher", Description = "$25 Starbucks gift card", Image = "coffee_icon", Type = RewardType.Voucher }
);

// Seed the Owned Type data separately for the Reward entity
builder.Entity<Reward>().OwnsOne(r => r.Cost).HasData(
    new { RewardId = Guid.Parse("00000000-0000-0000-0000-000000000007"), Value = 500 },
    new { RewardId = Guid.Parse("00000000-0000-0000-0000-000000000008"), Value = 750 },
    new { RewardId = Guid.Parse("00000000-0000-0000-0000-000000000009"), Value = 1000 },
    new { RewardId = Guid.Parse("00000000-0000-0000-0000-000000000010"), Value = 1500 }
);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            string? _userId = null;
            if (_httpContextAccessor?.HttpContext is not null)
            {
                var reqservices = _httpContextAccessor.HttpContext.RequestServices;
                if (reqservices is not null)
                    using (var scope = reqservices.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        if (services is not null)
                            try
                            {
                                var _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                                if (_httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated == true)
                                {
                                    var user = _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User).Result;
                                    _userId = user?.Id;
                                }
                            }
                            catch { }
                    }
            }

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).DateOfUpdate = DateTime.Now;
                ((BaseEntity)entityEntry.Entity).UpdatedById = _userId;
                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).DateOfCreattion = DateTime.Now;
                    ((BaseEntity)entityEntry.Entity).CreatedById = _userId;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        #region Community
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Photo> Photos { get; set; }
        #endregion
        

        #region Gamification
        public DbSet<UserGamificationProfile> UserGamificationProfiles { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        #endregion

        
        
        #region DBSets — Base
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<OtpEntry> OtpEntries { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
    
        #endregion

        #region DBSets — Blood Donation
        public DbSet<Donor> Donors { get; set; }
        public DbSet<HospitalAdmin> HospitalAdmins { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<BloodType> BloodTypes { get; set; }
        public DbSet<BloodTypeCompatibility> BloodTypeCompatibilities { get; set; }
        public DbSet<DonationRequest> DonationRequests { get; set; }
        public DbSet<RequestResponse> RequestResponses { get; set; }
        public DbSet<DonorNotificationLog> DonorNotificationLogs { get; set; }
        public DbSet<DonationHistory> DonationHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        #endregion
        /////
        ///
        #region DBSets — InventoryTables
        // ── Blood Inventory ───────────────────────────────────────────
        public DbSet<BloodInventory> BloodInventories { get; set; }
        public DbSet<BloodInventoryBatch> BloodInventoryBatches { get; set; }
        public DbSet<BloodInventoryTransaction> BloodInventoryTransactions { get; set; }
        #endregion
    }
}
