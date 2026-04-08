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
