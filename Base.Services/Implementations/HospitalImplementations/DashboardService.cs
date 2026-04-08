// Base.Services/Implementations/HospitalImplementations/DashboardService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.DashboardDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        // ── Helper: get hospitalId from admin user ────────────────
        private async Task<string> GetHospitalIdAsync(string hospitalAdminUserId)
        {
            var admin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (admin == null)
                throw new UnauthorizedAccessException(
                    "No hospital admin record found.");

            return admin.HospitalId;
        }

        // ── Helper: calculate % change ────────────────────────────
        private static double CalcChangePercent(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100.0 : 0.0;
            return Math.Round((double)(current - previous) / previous * 100, 1);
        }

        // ── Helper: get current and previous period boundaries ────
        // Current  = 1st of this month → today
        // Previous = 1st of last month → same day last month
        private static (DateTime currentStart, DateTime currentEnd,
                        DateTime previousStart, DateTime previousEnd)
            GetPeriodBoundaries()
        {
            var today        = DateTime.UtcNow.Date;
            var currentStart = new DateTime(today.Year, today.Month, 1);
            var currentEnd   = today.AddDays(1); // exclusive

            var previousStart = currentStart.AddMonths(-1);
            var previousEnd   = previousStart.AddDays(
                (today - currentStart).TotalDays + 1); // same relative day

            return (currentStart, currentEnd, previousStart, previousEnd);
        }

        // ── GET /api/hospital/dashboard/stats ─────────────────────
        public async Task<DashboardStatsDTO> GetStatsAsync(
            string hospitalAdminUserId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);
            var (currStart, currEnd, prevStart, prevEnd) = GetPeriodBoundaries();
            var today      = DateTime.UtcNow.Date;
            var tomorrow   = today.AddDays(1);

            // ── 1. Total Donors ───────────────────────────────────
            // All unique donors who ever responded to this hospital
            var allDonorIds = await _context.RequestResponses
                .AsNoTracking()
                .Where(rr =>
                    rr.Request.HospitalId == hospitalId &&
                    !rr.Request.IsDeleted)
                .Select(rr => rr.DonorId)
                .Distinct()
                .ToListAsync();

            int totalDonors = allDonorIds.Count;

            // Donors who responded for first time in current period
            var currDonors = await _context.RequestResponses
                .AsNoTracking()
                .Where(rr =>
                    rr.Request.HospitalId == hospitalId &&
                    !rr.Request.IsDeleted &&
                    rr.DateOfCreattion >= currStart &&
                    rr.DateOfCreattion <  currEnd)
                .Select(rr => rr.DonorId)
                .Distinct()
                .CountAsync();

            var prevDonors = await _context.RequestResponses
                .AsNoTracking()
                .Where(rr =>
                    rr.Request.HospitalId == hospitalId &&
                    !rr.Request.IsDeleted &&
                    rr.DateOfCreattion >= prevStart &&
                    rr.DateOfCreattion <  prevEnd)
                .Select(rr => rr.DonorId)
                .Distinct()
                .CountAsync();

            // ── 2. Available Blood Units ──────────────────────────
            var currentUnits = await _context.BloodInventories
                .AsNoTracking()
                .Where(i => i.HospitalId == hospitalId)
                .SumAsync(i => (int?)i.TotalUnits) ?? 0;

            // Previous period — sum units before current period
            // Approximate: current - units added this month + units removed this month
            var unitsAddedThisMonth = await _context.BloodInventoryTransactions
                .AsNoTracking()
                .Where(t =>
                    t.HospitalId == hospitalId &&
                    t.ChangeAmount > 0 &&
                    t.ChangedAt >= currStart &&
                    t.ChangedAt <  currEnd)
                .SumAsync(t => (int?)t.ChangeAmount) ?? 0;

            var unitsRemovedThisMonth = await _context.BloodInventoryTransactions
                .AsNoTracking()
                .Where(t =>
                    t.HospitalId == hospitalId &&
                    t.ChangeAmount < 0 &&
                    t.ChangedAt >= currStart &&
                    t.ChangedAt <  currEnd)
                .SumAsync(t => (int?)t.ChangeAmount) ?? 0;

            int previousUnits = currentUnits - unitsAddedThisMonth + Math.Abs(unitsRemovedThisMonth);

            // ── 3. Urgent (Critical) Requests ─────────────────────
            int urgentCurrent = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    r.HospitalId   == hospitalId &&
                    r.UrgencyLevel == UrgencyLevel.Critical &&
                    r.Status       == RequestStatus.Open &&
                    !r.IsDeleted);

            int urgentPrev = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    r.HospitalId   == hospitalId &&
                    r.UrgencyLevel == UrgencyLevel.Critical &&
                    r.Status       == RequestStatus.Open &&
                    !r.IsDeleted &&
                    r.DateOfCreattion >= prevStart &&
                    r.DateOfCreattion <  prevEnd);

            // ── 4. Transfusions Today ─────────────────────────────
            int transfusionsToday = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync(dh =>
                    dh.HospitalId    == hospitalId &&
                    dh.DonationDate  >= today &&
                    dh.DonationDate  <  tomorrow);

            // Same day last month
            var sameDayLastMonth      = today.AddMonths(-1);
            var sameDayLastMonthPlus1 = sameDayLastMonth.AddDays(1);

            int transfusionsPrevDay = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync(dh =>
                    dh.HospitalId   == hospitalId &&
                    dh.DonationDate >= sameDayLastMonth &&
                    dh.DonationDate <  sameDayLastMonthPlus1);

            return new DashboardStatsDTO
            {
                TotalDonors = new StatCardDTO
                {
                    Value         = totalDonors,
                    ChangePercent = CalcChangePercent(currDonors, prevDonors),
                    ChangeLabel   = "vs last month"
                },
                AvailableBloodUnits = new StatCardDTO
                {
                    Value         = currentUnits,
                    ChangePercent = CalcChangePercent(currentUnits, previousUnits),
                    ChangeLabel   = "vs last month"
                },
                UrgentRequests = new StatCardDTO
                {
                    Value         = urgentCurrent,
                    ChangePercent = CalcChangePercent(urgentCurrent, urgentPrev),
                    ChangeLabel   = "vs last month"
                },
                TransfusionsToday = new StatCardDTO
                {
                    Value         = transfusionsToday,
                    ChangePercent = CalcChangePercent(transfusionsToday, transfusionsPrevDay),
                    ChangeLabel   = "vs last month"
                }
            };
        }

        // ── Build activity items from all sources ─────────────────
        private async Task<List<ActivityItemDTO>> BuildActivitiesAsync(
            string hospitalId,
            string? activityTypeFilter,
            string? bloodTypeIdFilter,
            DateOnly? dateFilter,
            int skip,
            int take,
            bool countOnly = false)
        {
            var results = new List<ActivityItemDTO>();

            // ── Donation Histories ────────────────────────────────
            if (activityTypeFilter == null || activityTypeFilter == "Donation")
            {
                var dhQuery = _context.DonationHistories
                    .AsNoTracking()
                    .Where(dh => dh.HospitalId == hospitalId);

                if (!string.IsNullOrWhiteSpace(bloodTypeIdFilter))
                {
                    dhQuery = dhQuery.Where(dh =>
                        _context.Donors
                            .Where(d => d.Id == dh.DonorId)
                            .Select(d => d.BloodTypeId)
                            .FirstOrDefault() == bloodTypeIdFilter);
                }

                if (dateFilter.HasValue)
                {
                    var d = dateFilter.Value.ToDateTime(TimeOnly.MinValue);
                    dhQuery = dhQuery.Where(dh =>
                        dh.DonationDate >= d &&
                        dh.DonationDate <  d.AddDays(1));
                }

                var donations = await dhQuery
                    .OrderByDescending(dh => dh.DonationDate)
                    .Select(dh => new
                    {
                        dh.Id,
                        dh.DonationDate,
                        dh.DonorId,
                        BloodTypeId   = _context.Donors
                            .Where(d => d.Id == dh.DonorId)
                            .Select(d => d.BloodTypeId)
                            .FirstOrDefault(),
                        BloodTypeName = _context.Donors
                            .Where(d => d.Id == dh.DonorId)
                            .Select(d => d.BloodType.TypeName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                results.AddRange(donations.Select(dh => new ActivityItemDTO
                {
                    Id              = dh.Id,
                    ActivityType    = "BloodDonation",
                    Title           = $"Blood Donation: {dh.BloodTypeName ?? "Unknown"}",
                    BloodTypeId     = dh.BloodTypeId,
                    BloodTypeName   = dh.BloodTypeName,
                    RelatedId       = dh.Id,
                    OccurredAt      = dh.DonationDate,
                    TransactionHash = null,
                    IsVerified      = false
                }));
            }

            // ── Donation Requests ─────────────────────────────────
            if (activityTypeFilter == null ||
                activityTypeFilter == "Request")
            {
                var reqQuery = _context.DonationRequests
                    .AsNoTracking()
                    .Where(r => r.HospitalId == hospitalId && !r.IsDeleted);

                if (!string.IsNullOrWhiteSpace(bloodTypeIdFilter))
                    reqQuery = reqQuery.Where(r => r.BloodTypeId == bloodTypeIdFilter);

                if (dateFilter.HasValue)
                {
                    var d = dateFilter.Value.ToDateTime(TimeOnly.MinValue);
                    reqQuery = reqQuery.Where(r =>
                        r.DateOfCreattion >= d &&
                        r.DateOfCreattion <  d.AddDays(1));
                }

                var requests = await reqQuery
                    .OrderByDescending(r => r.DateOfCreattion)
                    .Select(r => new
                    {
                        r.Id,
                        r.Status,
                        r.BloodTypeId,
                        BloodTypeName = r.BloodType.TypeName,
                        r.UrgencyLevel,
                        r.DateOfCreattion,
                        r.DateOfUpdate
                    })
                    .ToListAsync();

                foreach (var r in requests)
                {
                    // New request created
                    results.Add(new ActivityItemDTO
                    {
                        Id              = r.Id + "_created",
                        ActivityType    = "NewRequest",
                        Title           = $"New Request: {r.BloodTypeName} {r.UrgencyLevel}",
                        BloodTypeId     = r.BloodTypeId,
                        BloodTypeName   = r.BloodTypeName,
                        RelatedId       = r.Id,
                        OccurredAt      = r.DateOfCreattion,
                        TransactionHash = null,
                        IsVerified      = false
                    });

                    // Fulfilled
                    if (r.Status == RequestStatus.Fulfilled)
                        results.Add(new ActivityItemDTO
                        {
                            Id              = r.Id + "_fulfilled",
                            ActivityType    = "RequestFulfilled",
                            Title           = $"Request Fulfilled: {r.BloodTypeName}",
                            BloodTypeId     = r.BloodTypeId,
                            BloodTypeName   = r.BloodTypeName,
                            RelatedId       = r.Id,
                            OccurredAt      = r.DateOfUpdate,
                            TransactionHash = null,
                            IsVerified      = false
                        });

                    // Cancelled
                    if (r.Status == RequestStatus.Cancelled)
                        results.Add(new ActivityItemDTO
                        {
                            Id              = r.Id + "_cancelled",
                            ActivityType    = "RequestCancelled",
                            Title           = $"Request Cancelled: {r.BloodTypeName}",
                            BloodTypeId     = r.BloodTypeId,
                            BloodTypeName   = r.BloodTypeName,
                            RelatedId       = r.Id,
                            OccurredAt      = r.DateOfUpdate,
                            TransactionHash = null,
                            IsVerified      = false
                        });
                }
            }

            // ── Inventory Transactions ────────────────────────────
            if (activityTypeFilter == null ||
                activityTypeFilter == "Inventory")
            {
                var invQuery = _context.BloodInventoryTransactions
                    .AsNoTracking()
                    .Where(t => t.HospitalId == hospitalId);

                if (!string.IsNullOrWhiteSpace(bloodTypeIdFilter))
                    invQuery = invQuery.Where(t => t.BloodTypeId == bloodTypeIdFilter);

                if (dateFilter.HasValue)
                {
                    var d = dateFilter.Value.ToDateTime(TimeOnly.MinValue);
                    invQuery = invQuery.Where(t =>
                        t.ChangedAt >= d &&
                        t.ChangedAt <  d.AddDays(1));
                }

                var transactions = await invQuery
                    .OrderByDescending(t => t.ChangedAt)
                    .Select(t => new
                    {
                        t.Id,
                        t.ChangeAmount,
                        t.Reason,
                        t.BloodTypeId,
                        BloodTypeName = t.BloodType.TypeName,
                        t.ChangedAt
                    })
                    .ToListAsync();

                results.AddRange(transactions.Select(t =>
                {
                    string actType = t.Reason switch
                    {
                        TransactionReason.ManualAdd          => "InventoryAdded",
                        TransactionReason.ManualWithdraw     => "InventoryWithdrawn",
                        TransactionReason.RequestFulfillment => "InventoryWithdrawn",
                        TransactionReason.ExpiredAutoRemoved => "BatchExpired",
                        TransactionReason.CompatibleTypeUsed => "InventoryWithdrawn",
                        _                                    => "InventoryAdded"
                    };

                    string title = t.Reason switch
                    {
                        TransactionReason.ManualAdd          =>
                            $"Inventory Added: {t.BloodTypeName} +{t.ChangeAmount} units",
                        TransactionReason.ManualWithdraw     =>
                            $"Inventory Withdrawn: {t.BloodTypeName} {t.ChangeAmount} units",
                        TransactionReason.RequestFulfillment =>
                            $"Fulfilled from Inventory: {t.BloodTypeName} {t.ChangeAmount} units",
                        TransactionReason.ExpiredAutoRemoved =>
                            $"Batch Expired: {t.BloodTypeName} {Math.Abs(t.ChangeAmount)} units",
                        _                                    =>
                            $"Inventory Change: {t.BloodTypeName}"
                    };

                    return new ActivityItemDTO
                    {
                        Id              = t.Id,
                        ActivityType    = actType,
                        Title           = title,
                        BloodTypeId     = t.BloodTypeId,
                        BloodTypeName   = t.BloodTypeName,
                        RelatedId       = t.Id,
                        OccurredAt      = t.ChangedAt,
                        TransactionHash = null,
                        IsVerified      = false
                    };
                }));
            }

            // Sort all results by most recent and paginate
            return results
                .OrderByDescending(a => a.OccurredAt)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        // ── GET /api/hospital/dashboard/recent-activity ───────────
        public async Task<RecentActivityResult> GetRecentActivityAsync(
            string hospitalAdminUserId,
            int limit = 4)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            var activities = await BuildActivitiesAsync(
                hospitalId,
                activityTypeFilter: null,
                bloodTypeIdFilter:  null,
                dateFilter:         null,
                skip:               0,
                take:               limit);

            return new RecentActivityResult
            {
                Success = true,
                Message = "Recent activity retrieved successfully.",
                Value   = activities
            };
        }

        // ── GET /api/hospital/dashboard/activity-log ──────────────
        public async Task<ActivityLogResult> GetActivityLogAsync(
            string hospitalAdminUserId,
            ActivityLogQuery query)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            // Get total count — fetch all then count
            var allActivities = await BuildActivitiesAsync(
                hospitalId,
                activityTypeFilter: query.ActivityType,
                bloodTypeIdFilter:  query.BloodTypeId,
                dateFilter:         query.Date,
                skip:               0,
                take:               int.MaxValue);

            int total = allActivities.Count;

            // Apply pagination on sorted list
            var paged = allActivities
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToList();

            return new ActivityLogResult
            {
                Success    = true,
                Message    = "Activity log retrieved successfully.",
                Total      = total,
                Page       = query.Page,
                Limit      = query.Limit,
                TotalPages = (int)Math.Ceiling((double)total / query.Limit),
                Value      = paged
            };
        }
    }
}
