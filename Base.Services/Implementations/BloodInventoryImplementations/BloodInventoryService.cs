// Base.Services/Implementations/HospitalImplementations/BloodInventoryService.cs

using Base.DAL.Contexts;
using Base.DAL.Models.InventoryModels;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.InventoryDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class BloodInventoryService : IBloodInventoryService
    {
        private readonly AppDbContext _context;

        public BloodInventoryService(AppDbContext context)
        {
            _context = context;
        }

        // ── Helper: get hospital from admin user ──────────────────
        private async Task<(string hospitalId, string adminUserId)> GetHospitalAsync(
            string hospitalAdminUserId)
        {
            var admin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (admin == null)
                throw new UnauthorizedAccessException(
                    "No hospital admin record found.");

            return (admin.HospitalId, hospitalAdminUserId);
        }

        // ── Helper: get or create inventory record ────────────────
        private async Task<BloodInventory> GetOrCreateInventoryAsync(
            string hospitalId,
            string bloodTypeId)
        {
            var inventory = await _context.BloodInventories
                .FirstOrDefaultAsync(i =>
                    i.HospitalId == hospitalId &&
                    i.BloodTypeId == bloodTypeId);

            if (inventory != null) return inventory;

            inventory = new BloodInventory
            {
                HospitalId   = hospitalId,
                BloodTypeId  = bloodTypeId,
                TotalUnits   = 0,
                LastUpdated  = DateTime.UtcNow
            };

            _context.BloodInventories.Add(inventory);
            await _context.SaveChangesAsync();

            return inventory;
        }

        // ── Helper: recalculate TotalUnits from active batches ────
        private async Task RecalculateTotalUnitsAsync(string inventoryId)
        {
            var total = await _context.BloodInventoryBatches
                .Where(b =>
                    b.BloodInventoryId == inventoryId &&
                    b.Status == BatchStatus.Active)
                .SumAsync(b => b.RemainingUnits);

            var inventory = await _context.BloodInventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory != null)
            {
                inventory.TotalUnits  = total;
                inventory.LastUpdated = DateTime.UtcNow;
            }
        }

        // ── GET /api/hospital/inventory ───────────────────────────
        public async Task<InventoryOverviewResult> GetInventoryAsync(
            string hospitalAdminUserId)
        {
            var (hospitalId, _) = await GetHospitalAsync(hospitalAdminUserId);

            var today       = DateOnly.FromDateTime(DateTime.UtcNow);
            var in7Days     = today.AddDays(7);

            var inventories = await _context.BloodInventories
                .AsNoTracking()
                .Where(i => i.HospitalId == hospitalId)
                .Select(i => new InventoryItemDTO
                {
                    InventoryId     = i.Id,
                    BloodTypeId     = i.BloodTypeId,
                    BloodTypeName   = i.BloodType.TypeName,
                    TotalUnits      = i.TotalUnits,
                    BatchCount      = i.Batches.Count(b => b.Status == BatchStatus.Active),
                    ExpiringIn7Days = i.Batches.Count(b =>
                        b.Status == BatchStatus.Active &&
                        b.ExpiryDate <= in7Days),
                    NearestExpiry   = i.Batches
                        .Where(b => b.Status == BatchStatus.Active)
                        .OrderBy(b => b.ExpiryDate)
                        .Select(b => (DateOnly?)b.ExpiryDate)
                        .FirstOrDefault()
                })
                .OrderBy(i => i.BloodTypeName)
                .ToListAsync();

            return new InventoryOverviewResult
            {
                Success = true,
                Message = "Inventory retrieved successfully.",
                Value   = inventories
            };
        }

        // ── GET /api/hospital/inventory/{bloodTypeId} ─────────────
        public async Task<InventoryDetailResult> GetInventoryByBloodTypeAsync(
            string hospitalAdminUserId,
            string bloodTypeId)
        {
            var (hospitalId, _) = await GetHospitalAsync(hospitalAdminUserId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var inventory = await _context.BloodInventories
                .AsNoTracking()
                .Where(i =>
                    i.HospitalId  == hospitalId &&
                    i.BloodTypeId == bloodTypeId)
                .Select(i => new InventoryDetailDTO
                {
                    InventoryId   = i.Id,
                    BloodTypeId   = i.BloodTypeId,
                    BloodTypeName = i.BloodType.TypeName,
                    TotalUnits    = i.TotalUnits,
                    Batches       = i.Batches
                        .Where(b => b.Status == BatchStatus.Active)
                        .OrderBy(b => b.ExpiryDate)
                        .Select(b => new BatchSummaryDTO
                        {
                            Id              = b.Id,
                            Units           = b.Units,
                            RemainingUnits  = b.RemainingUnits,
                            CollectionDate  = b.CollectionDate,
                            ExpiryDate      = b.ExpiryDate,
                            DaysUntilExpiry = b.ExpiryDate.DayNumber - today.DayNumber,
                            Status          = b.Status
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (inventory == null)
                return new InventoryDetailResult
                {
                    Success = false,
                    Message = "No inventory found for this blood type."
                };

            return new InventoryDetailResult
            {
                Success = true,
                Message = "Inventory retrieved successfully.",
                Value   = inventory
            };
        }

        // ── POST /api/hospital/inventory/batches/add ──────────────
        public async Task<AddBatchResult> AddBatchAsync(
            string hospitalAdminUserId,
            AddBatchDTO dto)
        {
            var (hospitalId, userId) = await GetHospitalAsync(hospitalAdminUserId);

            // Validate dates
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (dto.ExpiryDate <= today)
                throw new ArgumentException(
                    "Expiry date must be in the future.");

            if (dto.CollectionDate > today)
                throw new ArgumentException(
                    "Collection date cannot be in the future.");

            if (dto.ExpiryDate <= dto.CollectionDate)
                throw new ArgumentException(
                    "Expiry date must be after collection date.");

            // Validate blood type
            var bloodTypeExists = await _context.BloodTypes
                .AnyAsync(b => b.Id == dto.BloodTypeId);

            if (!bloodTypeExists)
                throw new ArgumentException("Invalid blood type.");

            // Get or create inventory record
            var inventory = await GetOrCreateInventoryAsync(
                hospitalId, dto.BloodTypeId);

            // Create batch
            var batch = new BloodInventoryBatch
            {
                HospitalId       = hospitalId,
                BloodTypeId      = dto.BloodTypeId,
                BloodInventoryId = inventory.Id,
                Units            = dto.Units,
                RemainingUnits   = dto.Units,
                CollectionDate   = dto.CollectionDate,
                ExpiryDate       = dto.ExpiryDate,
                Status           = BatchStatus.Active
            };

            _context.BloodInventoryBatches.Add(batch);

            // Create transaction record
            var transaction = new BloodInventoryTransaction
            {
                HospitalId       = hospitalId,
                BloodTypeId      = dto.BloodTypeId,
                BloodInventoryId = inventory.Id,
                ChangeAmount     = dto.Units,
                Reason           = TransactionReason.ManualAdd,
                ChangedById      = userId,
                ChangedAt        = DateTime.UtcNow,
                Notes            = dto.Notes
            };

            _context.BloodInventoryTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            // Recalculate total units
            await RecalculateTotalUnitsAsync(inventory.Id);
            await _context.SaveChangesAsync();

            return new AddBatchResult
            {
                Success = true,
                Message = $"{dto.Units} units of blood added successfully.",
                Value   = new BatchSummaryDTO
                {
                    Id              = batch.Id,
                    Units           = batch.Units,
                    RemainingUnits  = batch.RemainingUnits,
                    CollectionDate  = batch.CollectionDate,
                    ExpiryDate      = batch.ExpiryDate,
                    DaysUntilExpiry = batch.ExpiryDate.DayNumber - today.DayNumber,
                    Status          = batch.Status
                }
            };
        }

        // ── POST /api/hospital/inventory/withdraw ─────────────────
        public async Task<WithdrawResult> WithdrawAsync(
          string hospitalAdminUserId,
          WithdrawDTO dto)
        {
            var (hospitalId, userId) = await GetHospitalAsync(hospitalAdminUserId);

            // Get inventory
            var inventory = await _context.BloodInventories
                .FirstOrDefaultAsync(i =>
                    i.HospitalId == hospitalId &&
                    i.BloodTypeId == dto.BloodTypeId);

            if (inventory == null || inventory.TotalUnits == 0)
                throw new InvalidOperationException(
                    "No inventory available for this blood type.");

            if (inventory.TotalUnits < dto.Units)
                throw new InvalidOperationException(
                    $"Insufficient units. Available: {inventory.TotalUnits}, Requested: {dto.Units}.");

            // Deduct from batches — FIFO (oldest expiry first)
            var batches = await _context.BloodInventoryBatches
                .Where(b =>
                    b.BloodInventoryId == inventory.Id &&
                    b.Status == BatchStatus.Active &&
                    b.RemainingUnits > 0)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();

            int remaining = dto.Units;

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                int deduct = Math.Min(batch.RemainingUnits, remaining);
                batch.RemainingUnits -= deduct;
                remaining -= deduct;

                if (batch.RemainingUnits == 0)
                    batch.Status = BatchStatus.Depleted;
            }

            // ── Update QuantityFulfilled if linked to a request ───────────
            if (!string.IsNullOrWhiteSpace(dto.RequestId))
            {
                var request = await _context.DonationRequests
                    .FirstOrDefaultAsync(r =>
                        r.Id == dto.RequestId &&
                        r.HospitalId == hospitalId);

                if (request == null)
                    throw new KeyNotFoundException(
                        "Linked donation request not found.");

                if (request.Status != RequestStatus.Open)
                    throw new InvalidOperationException(
                        "Cannot fulfill a request that is not Open.");

                request.QuantityFulfilled += dto.Units;

                // Auto-close request if fully fulfilled
                if (request.QuantityFulfilled >= request.QuantityRequired)
                    request.Status = RequestStatus.Fulfilled;
            }

            // ── Create transaction record ─────────────────────────────────
            var transaction = new BloodInventoryTransaction
            {
                HospitalId = hospitalId,
                BloodTypeId = dto.BloodTypeId,
                BloodInventoryId = inventory.Id,
                ChangeAmount = -dto.Units,
                Reason = !string.IsNullOrWhiteSpace(dto.RequestId)
                                   ? TransactionReason.RequestFulfillment
                                   : TransactionReason.ManualWithdraw,
                RequestId = dto.RequestId,
                ChangedById = userId,
                ChangedAt = DateTime.UtcNow,
                Notes = dto.Notes
            };

            _context.BloodInventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // ── Recalculate total units ───────────────────────────────────
            await RecalculateTotalUnitsAsync(inventory.Id);
            await _context.SaveChangesAsync();

            // ── Get blood type name ───────────────────────────────────────
            var bloodTypeName = await _context.BloodTypes
                .AsNoTracking()
                .Where(b => b.Id == dto.BloodTypeId)
                .Select(b => b.TypeName)
                .FirstOrDefaultAsync() ?? dto.BloodTypeId;

            return new WithdrawResult
            {
                Success = true,
                Message = $"{dto.Units} units withdrawn successfully.",
                BloodTypeId = dto.BloodTypeId,
                BloodTypeName = bloodTypeName,
                UnitsWithdrawn = dto.Units,
                RemainingUnits = inventory.TotalUnits
            };
        }
        // ── GET /api/hospital/inventory/expiring-soon ─────────────
        public async Task<ExpiringSoonResult> GetExpiringSoonAsync(
            string hospitalAdminUserId,
            int days = 7)
        {
            var (hospitalId, _) = await GetHospitalAsync(hospitalAdminUserId);

            var today    = DateOnly.FromDateTime(DateTime.UtcNow);
            var deadline = today.AddDays(days);

            var batches = await _context.BloodInventoryBatches
                .AsNoTracking()
                .Where(b =>
                    b.HospitalId      == hospitalId &&
                    b.Status          == BatchStatus.Active &&
                    b.RemainingUnits  > 0 &&
                    b.ExpiryDate      <= deadline)
                .OrderBy(b => b.ExpiryDate)
                .Select(b => new ExpiringSoonDTO
                {
                    BatchId         = b.Id,
                    BloodTypeId     = b.BloodTypeId,
                    BloodTypeName   = b.BloodType.TypeName,
                    RemainingUnits  = b.RemainingUnits,
                    ExpiryDate      = b.ExpiryDate,
                    DaysUntilExpiry = b.ExpiryDate.DayNumber - today.DayNumber
                })
                .ToListAsync();

            return new ExpiringSoonResult
            {
                Success = true,
                Message = batches.Any()
                    ? $"{batches.Count} batch(es) expiring within {days} days."
                    : "No batches expiring soon.",
                Value   = batches
            };
        }

        // ── GET /api/hospital/inventory/transactions ──────────────
        public async Task<TransactionListResult> GetTransactionsAsync(
            string hospitalAdminUserId,
            int page  = 1,
            int limit = 10)
        {
            var (hospitalId, _) = await GetHospitalAsync(hospitalAdminUserId);

            var total = await _context.BloodInventoryTransactions
                .AsNoTracking()
                .CountAsync(t => t.HospitalId == hospitalId);

            var transactions = await _context.BloodInventoryTransactions
                .AsNoTracking()
                .Where(t => t.HospitalId == hospitalId)
                .OrderByDescending(t => t.ChangedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(t => new TransactionDTO
                {
                    Id            = t.Id,
                    BloodTypeId   = t.BloodTypeId,
                    BloodTypeName = t.BloodType.TypeName,
                    ChangeAmount  = t.ChangeAmount,
                    Reason        = t.Reason,
                    RequestId     = t.RequestId,
                    Notes         = t.Notes,
                    ChangedAt     = t.ChangedAt
                })
                .ToListAsync();

            return new TransactionListResult
            {
                Success    = true,
                Message    = "Transactions retrieved successfully.",
                Total      = total,
                Page       = page,
                Limit      = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit),
                Value      = transactions
            };
        }

        // ── GET /api/hospital/inventory/compatible/{bloodTypeId} ──
        public async Task<CompatibleInventoryResult> GetCompatibleInventoryAsync(
            string hospitalAdminUserId,
            string bloodTypeId)
        {
            var (hospitalId, _) = await GetHospitalAsync(hospitalAdminUserId);

            // Get exact match first
            var exactInventory = await _context.BloodInventories
                .AsNoTracking()
                .Where(i =>
                    i.HospitalId  == hospitalId &&
                    i.BloodTypeId == bloodTypeId)
                .Select(i => new { i.TotalUnits, i.BloodTypeId, i.BloodType.TypeName })
                .FirstOrDefaultAsync();

            int exactUnits = exactInventory?.TotalUnits ?? 0;

            // Get compatible blood type IDs from BloodTypeCompatibility table
            var compatibleTypeIds = await _context.BloodTypeCompatibilities
                .AsNoTracking()
                .Where(c => c.RecipientBloodTypeId == bloodTypeId)
                .Select(c => c.DonorBloodTypeId)
                .ToListAsync();

            // Remove exact match from compatible list
            compatibleTypeIds = compatibleTypeIds
                .Where(id => id != bloodTypeId)
                .ToList();

            // Get inventory for compatible types
            var compatibleInventories = await _context.BloodInventories
                .AsNoTracking()
                .Where(i =>
                    i.HospitalId == hospitalId &&
                    compatibleTypeIds.Contains(i.BloodTypeId) &&
                    i.TotalUnits > 0)
                .Select(i => new CompatibleInventoryDTO
                {
                    BloodTypeId    = i.BloodTypeId,
                    BloodTypeName  = i.BloodType.TypeName,
                    AvailableUnits = i.TotalUnits,
                    IsExactMatch   = false
                })
                .ToListAsync();

            // Build result — exact match first
            var allAvailable = new List<CompatibleInventoryDTO>();

            if (exactUnits > 0)
                allAvailable.Add(new CompatibleInventoryDTO
                {
                    BloodTypeId    = bloodTypeId,
                    BloodTypeName  = exactInventory!.TypeName,
                    AvailableUnits = exactUnits,
                    IsExactMatch   = true
                });

            allAvailable.AddRange(compatibleInventories);

            int totalAvailable = allAvailable.Sum(x => x.AvailableUnits);

            var requestedTypeName = await _context.BloodTypes
                .AsNoTracking()
                .Where(b => b.Id == bloodTypeId)
                .Select(b => b.TypeName)
                .FirstOrDefaultAsync() ?? bloodTypeId;

            return new CompatibleInventoryResult
            {
                Success        = true,
                Message        = totalAvailable > 0
                                 ? $"{totalAvailable} total units available."
                                 : "No inventory available for this blood type or compatible types.",
                RequestedType  = requestedTypeName,
                ExactUnits     = exactUnits,
                Compatible     = allAvailable,
                TotalAvailable = totalAvailable
            };
        }
    }
}
