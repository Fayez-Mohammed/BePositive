// Base.Services/Implementations/HospitalImplementations/AnalyticsService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.AnalyticsDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        // ── Helper: get hospitalId ────────────────────────────────
        private async Task<string> GetHospitalIdAsync(string userId)
        {
            var admin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == userId && !ha.IsDeleted);

            if (admin == null)
                throw new UnauthorizedAccessException(
                    "No hospital admin record found.");

            return admin.HospitalId;
        }

        // ── Helper: get date ranges for current and previous period
        private static (DateTime currStart, DateTime currEnd,
                         DateTime prevStart, DateTime prevEnd,
                         string periodLabel)
            GetDateRanges(AnalyticsPeriod period)
        {
            var now = DateTime.UtcNow;

            return period switch
            {
                AnalyticsPeriod.Last7Days => (
                    now.AddDays(-7).Date,
                    now.Date.AddDays(1),
                    now.AddDays(-14).Date,
                    now.AddDays(-7).Date,
                    "Last 7 Days"
                ),
                AnalyticsPeriod.Last30Days => (
                    now.AddDays(-30).Date,
                    now.Date.AddDays(1),
                    now.AddDays(-60).Date,
                    now.AddDays(-30).Date,
                    "Last 30 Days"
                ),
                AnalyticsPeriod.Last3Months => (
                    now.AddMonths(-3).Date,
                    now.Date.AddDays(1),
                    now.AddMonths(-6).Date,
                    now.AddMonths(-3).Date,
                    "Last 3 Months"
                ),
                AnalyticsPeriod.LastYear => (
                    now.AddYears(-1).Date,
                    now.Date.AddDays(1),
                    now.AddYears(-2).Date,
                    now.AddYears(-1).Date,
                    "Last Year"
                ),
                _ => (
                    now.AddDays(-7).Date,
                    now.Date.AddDays(1),
                    now.AddDays(-14).Date,
                    now.AddDays(-7).Date,
                    "Last 7 Days"
                )
            };
        }

        // ── Helper: % change ──────────────────────────────────────
        private static double CalcChange(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100.0 : 0.0;
            return Math.Round((current - previous) / previous * 100, 1);
        }

        // ── GET /api/hospital/analytics/summary ───────────────────
        public async Task<AnalyticsSummaryDTO> GetSummaryAsync(
            string hospitalAdminUserId,
            AnalyticsPeriod period)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);
            var (currStart, currEnd, prevStart, prevEnd, label) =
                GetDateRanges(period);

            // ── 1. Total Donations ────────────────────────────────
            int currDonations = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync(dh =>
                    dh.HospitalId == hospitalId &&
                    dh.DonationDate >= currStart &&
                    dh.DonationDate < currEnd);

            int prevDonations = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync(dh =>
                    dh.HospitalId == hospitalId &&
                    dh.DonationDate >= prevStart &&
                    dh.DonationDate < prevEnd);

            // ── 2. Response Rate ──────────────────────────────────
            // (Requests with at least 1 response / Total requests) * 100

            int currTotalRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    r.HospitalId == hospitalId &&
                    !r.IsDeleted &&
                    r.DateOfCreattion >= currStart &&
                    r.DateOfCreattion < currEnd);

            int currRespondedRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    r.HospitalId == hospitalId &&
                    !r.IsDeleted &&
                    r.DateOfCreattion >= currStart &&
                    r.DateOfCreattion < currEnd &&
                    _context.RequestResponses.Any(rr => rr.RequestId == r.Id));

            int prevTotalRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    r.HospitalId == hospitalId &&
                    !r.IsDeleted &&
                    r.DateOfCreattion >= prevStart &&
                    r.DateOfCreattion < prevEnd);

            int prevRespondedRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    r.HospitalId == hospitalId &&
                    !r.IsDeleted &&
                    r.DateOfCreattion >= prevStart &&
                    r.DateOfCreattion < prevEnd &&
                    _context.RequestResponses.Any(rr => rr.RequestId == r.Id));

            double currResponseRate = currTotalRequests == 0 ? 0
                : Math.Round((double)currRespondedRequests / currTotalRequests * 100, 1);

            double prevResponseRate = prevTotalRequests == 0 ? 0
                : Math.Round((double)prevRespondedRequests / prevTotalRequests * 100, 1);

            // ── 3. Avg Fulfillment Time ───────────────────────────
            // Time from request created to first Donated response
            var fulfilledRequests = await _context.DonationRequests
                .AsNoTracking()
                .Where(r =>
                    r.HospitalId == hospitalId &&
                    !r.IsDeleted &&
                    r.DateOfCreattion >= currStart &&
                    r.DateOfCreattion < currEnd)
                .Select(r => new
                {
                    r.Id,
                    r.DateOfCreattion,
                    FirstDonatedAt = _context.RequestResponses
                        .Where(rr =>
                            rr.RequestId == r.Id &&
                            rr.Status == ResponseStatus.Donated)
                        .OrderBy(rr => rr.DateOfCreattion)
                        .Select(rr => (DateTime?)rr.DateOfCreattion)
                        .FirstOrDefault()
                })
                .Where(r => r.FirstDonatedAt != null)
                .ToListAsync();

            double currAvgHours = fulfilledRequests.Any()
                ? Math.Round(fulfilledRequests
                    .Average(r => (r.FirstDonatedAt!.Value - r.DateOfCreattion).TotalHours), 1)
                : 0;

            var prevFulfilledRequests = await _context.DonationRequests
                .AsNoTracking()
                .Where(r =>
                    r.HospitalId == hospitalId &&
                    !r.IsDeleted &&
                    r.DateOfCreattion >= prevStart &&
                    r.DateOfCreattion < prevEnd)
                .Select(r => new
                {
                    r.Id,
                    r.DateOfCreattion,
                    FirstDonatedAt = _context.RequestResponses
                        .Where(rr =>
                            rr.RequestId == r.Id &&
                            rr.Status == ResponseStatus.Donated)
                        .OrderBy(rr => rr.DateOfCreattion)
                        .Select(rr => (DateTime?)rr.DateOfCreattion)
                        .FirstOrDefault()
                })
                .Where(r => r.FirstDonatedAt != null)
                .ToListAsync();

            double prevAvgHours = prevFulfilledRequests.Any()
                ? Math.Round(prevFulfilledRequests
                    .Average(r => (r.FirstDonatedAt!.Value - r.DateOfCreattion).TotalHours), 1)
                : 0;

            return new AnalyticsSummaryDTO
            {
                PeriodLabel = label,
                TotalDonations = new StatDTO
                {
                    Value = currDonations,
                    ChangePercent = CalcChange(currDonations, prevDonations)
                },
                ResponseRate = new StatDTO
                {
                    Value = currResponseRate,
                    ChangePercent = CalcChange(currResponseRate, prevResponseRate)
                },
                AvgFulfillmentHrs = new StatDTO
                {
                    Value = currAvgHours,
                    ChangePercent = CalcChange(currAvgHours, prevAvgHours)
                }
            };
        }
        // ── GET /api/hospital/analytics/trends ────────────────────
        public async Task<TrendsDTO> GetTrendsAsync(
            string hospitalAdminUserId,
            AnalyticsPeriod period)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);
            var (currStart, currEnd, _, _, _) = GetDateRanges(period);

            var result = new TrendsDTO();

            if (period == AnalyticsPeriod.Last7Days)
            {
                // 7 daily points
                for (int i = 6; i >= 0; i--)
                {
                    var day      = DateTime.UtcNow.Date.AddDays(-i);
                    var dayEnd   = day.AddDays(1);
                    var dayLabel = day.ToString("ddd"); // Mon, Tue...

                    int donations = await _context.DonationHistories
                        .AsNoTracking()
                        .CountAsync(dh =>
                            dh.HospitalId   == hospitalId &&
                            dh.DonationDate >= day &&
                            dh.DonationDate <  dayEnd);

                    int requests = await _context.DonationRequests
                        .AsNoTracking()
                        .CountAsync(r =>
                            r.HospitalId      == hospitalId &&
                            !r.IsDeleted &&
                            r.DateOfCreattion >= day &&
                            r.DateOfCreattion <  dayEnd);

                    result.Labels.Add(dayLabel);
                    result.Donations.Add(donations);
                    result.Requests.Add(requests);
                }
            }
            else if (period == AnalyticsPeriod.Last30Days)
            {
                // 30 daily points
                for (int i = 29; i >= 0; i--)
                {
                    var day    = DateTime.UtcNow.Date.AddDays(-i);
                    var dayEnd = day.AddDays(1);

                    int donations = await _context.DonationHistories
                        .AsNoTracking()
                        .CountAsync(dh =>
                            dh.HospitalId   == hospitalId &&
                            dh.DonationDate >= day &&
                            dh.DonationDate <  dayEnd);

                    int requests = await _context.DonationRequests
                        .AsNoTracking()
                        .CountAsync(r =>
                            r.HospitalId      == hospitalId &&
                            !r.IsDeleted &&
                            r.DateOfCreattion >= day &&
                            r.DateOfCreattion <  dayEnd);

                    result.Labels.Add(day.ToString("MMM d")); // Apr 1
                    result.Donations.Add(donations);
                    result.Requests.Add(requests);
                }
            }
            else if (period == AnalyticsPeriod.Last3Months)
            {
                // 12 weekly points
                for (int i = 11; i >= 0; i--)
                {
                    var weekStart = DateTime.UtcNow.Date.AddDays(-(i * 7) - 6);
                    var weekEnd   = weekStart.AddDays(7);

                    int donations = await _context.DonationHistories
                        .AsNoTracking()
                        .CountAsync(dh =>
                            dh.HospitalId   == hospitalId &&
                            dh.DonationDate >= weekStart &&
                            dh.DonationDate <  weekEnd);

                    int requests = await _context.DonationRequests
                        .AsNoTracking()
                        .CountAsync(r =>
                            r.HospitalId      == hospitalId &&
                            !r.IsDeleted &&
                            r.DateOfCreattion >= weekStart &&
                            r.DateOfCreattion <  weekEnd);

                    result.Labels.Add($"W{12 - i}");
                    result.Donations.Add(donations);
                    result.Requests.Add(requests);
                }
            }
            else // LastYear
            {
                // 12 monthly points
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = new DateTime(
                        DateTime.UtcNow.Year,
                        DateTime.UtcNow.Month, 1)
                        .AddMonths(-i);
                    var monthEnd = monthStart.AddMonths(1);

                    int donations = await _context.DonationHistories
                        .AsNoTracking()
                        .CountAsync(dh =>
                            dh.HospitalId   == hospitalId &&
                            dh.DonationDate >= monthStart &&
                            dh.DonationDate <  monthEnd);

                    int requests = await _context.DonationRequests
                        .AsNoTracking()
                        .CountAsync(r =>
                            r.HospitalId      == hospitalId &&
                            !r.IsDeleted &&
                            r.DateOfCreattion >= monthStart &&
                            r.DateOfCreattion <  monthEnd);

                    result.Labels.Add(monthStart.ToString("MMM")); // Jan
                    result.Donations.Add(donations);
                    result.Requests.Add(requests);
                }
            }

            return result;
        }

        // ── GET /api/hospital/analytics/blood-type-distribution ───
        public async Task<BloodTypeDistributionResult> GetBloodTypeDistributionAsync(
            string hospitalAdminUserId,
            AnalyticsPeriod period)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);
            var (currStart, currEnd, _, _, _) = GetDateRanges(period);

            // Count donations per blood type in the period
            var donations = await _context.DonationHistories
                .AsNoTracking()
                .Where(dh =>
                    dh.HospitalId   == hospitalId &&
                    dh.DonationDate >= currStart &&
                    dh.DonationDate <  currEnd)
                .Select(dh => new
                {
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

            var grouped = donations
                .Where(d => d.BloodTypeId != null)
                .GroupBy(d => new { d.BloodTypeId, d.BloodTypeName })
                .Select(g => new { g.Key.BloodTypeId, g.Key.BloodTypeName, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            int total = grouped.Sum(g => g.Count);

            var distribution = grouped.Select(g => new BloodTypeDistributionDTO
            {
                BloodTypeId   = g.BloodTypeId!,
                BloodTypeName = g.BloodTypeName ?? g.BloodTypeId!,
                Count         = g.Count,
                Percentage    = total == 0 ? 0
                    : Math.Round((double)g.Count / total * 100, 1)
            }).ToList();

            return new BloodTypeDistributionResult
            {
                Success = true,
                Message = "Blood type distribution retrieved successfully.",
                Value   = distribution
            };
        }
    }
}
