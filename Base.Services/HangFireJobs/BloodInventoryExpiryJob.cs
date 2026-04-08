// Base.Services/HangFireJobs/BloodInventoryExpiryJob.cs

using Base.DAL.Contexts;
using Base.DAL.Models.InventoryModels;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Base.Services.HangFireJobs
{
    public class BloodInventoryExpiryJob
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BloodInventoryExpiryJob> _logger;

        public BloodInventoryExpiryJob(
            AppDbContext context,
            ILogger<BloodInventoryExpiryJob> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation(
                "BloodInventoryExpiryJob started at {Time}", DateTime.UtcNow);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Find all active batches that have expired
            var expiredBatches = await _context.BloodInventoryBatches
                .Where(b =>
                    b.Status     == BatchStatus.Active &&
                    b.ExpiryDate <  today)
                .ToListAsync();

            if (!expiredBatches.Any())
            {
                _logger.LogInformation("No expired batches found.");
                return;
            }

            _logger.LogInformation(
                "Found {Count} expired batches.", expiredBatches.Count);

            // Group by inventory to create one transaction per inventory
            var groupedByInventory = expiredBatches
                .GroupBy(b => b.BloodInventoryId);

            foreach (var group in groupedByInventory)
            {
                var inventoryId   = group.Key;
                int totalExpired  = group.Sum(b => b.RemainingUnits);
                var bloodTypeId   = group.First().BloodTypeId;
                var hospitalId    = group.First().HospitalId;

                // Mark batches as expired
                foreach (var batch in group)
                {
                    batch.Status         = BatchStatus.Expired;
                    batch.RemainingUnits = 0;
                }

                // Create audit transaction record
                var transaction = new BloodInventoryTransaction
                {
                    HospitalId       = hospitalId,
                    BloodTypeId      = bloodTypeId,
                    BloodInventoryId = inventoryId,
                    ChangeAmount     = -totalExpired,
                    Reason           = TransactionReason.ExpiredAutoRemoved,
                    ChangedById      = "system",
                    ChangedAt        = DateTime.UtcNow,
                    Notes            = $"{group.Count()} batch(es) auto-expired on {today}."
                };

                _context.BloodInventoryTransactions.Add(transaction);

                // Recalculate TotalUnits for this inventory
                var inventory = await _context.BloodInventories
                    .FirstOrDefaultAsync(i => i.Id == inventoryId);

                if (inventory != null)
                {
                    inventory.TotalUnits = await _context.BloodInventoryBatches
                        .Where(b =>
                            b.BloodInventoryId == inventoryId &&
                            b.Status           == BatchStatus.Active)
                        .SumAsync(b => b.RemainingUnits);

                    inventory.LastUpdated = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "BloodInventoryExpiryJob completed. {Count} batches expired.",
                expiredBatches.Count);
        }
    }
}
