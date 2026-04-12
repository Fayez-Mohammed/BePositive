// Base.Services/Implementations/AdminAnalyticsService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations
{
    public class AdminAnalyticsService : IAdminAnalyticsService
    {
        private readonly AppDbContext _context;

        public AdminAnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        // ── GET /api/admin/analytics/summary ──────────────────
        public async Task<AdminAnalyticsSummaryDTO> GetSummaryAsync()
        {
            var totalDonations = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync();

            var totalRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r => !r.IsDeleted);

            var totalHospitals = await _context.Hospitals
                .AsNoTracking()
                .CountAsync(h => !h.IsDeleted);

            var totalDonors = await _context.Donors
                .AsNoTracking()
                .CountAsync(d => !d.IsDeleted);

            // Most requested blood type
            var mostRequested = await _context.DonationRequests
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .GroupBy(r => r.BloodTypeId)
                .Select(g => new
                {
                    BloodTypeId = g.Key,
                    Count       = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            string mostRequestedBloodType = "N/A";
            if (mostRequested != null)
            {
                mostRequestedBloodType = await _context.BloodTypes
                    .AsNoTracking()
                    .Where(b => b.Id == mostRequested.BloodTypeId)
                    .Select(b => b.TypeName)
                    .FirstOrDefaultAsync() ?? "N/A";
            }

            // Most active hospital (most donations)
            var mostActive = await _context.DonationHistories
                .AsNoTracking()
                .GroupBy(dh => dh.HospitalId)
                .Select(g => new
                {
                    HospitalId = g.Key,
                    Count      = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            string mostActiveHospital = "N/A";
            if (mostActive?.HospitalId != null)
            {
                mostActiveHospital = await _context.Hospitals
                    .AsNoTracking()
                    .Where(h => h.Id == mostActive.HospitalId)
                    .Select(h => h.Name)
                    .FirstOrDefaultAsync() ?? "N/A";
            }

            return new AdminAnalyticsSummaryDTO
            {
                TotalDonationsAllTime  = totalDonations,
                TotalRequestsAllTime   = totalRequests,
                TotalHospitals         = totalHospitals,
                TotalDonors            = totalDonors,
                MostRequestedBloodType = mostRequestedBloodType,
                MostActiveHospital     = mostActiveHospital
            };
        }

        // ── GET /api/admin/analytics/donations-trend ──────────
        public async Task<AdminDonationsTrendDTO> GetDonationsTrendAsync(
            string period = "LastYear")
        {
            var result = new AdminDonationsTrendDTO();

            // Always 12 monthly points
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
                        dh.DonationDate >= monthStart &&
                        dh.DonationDate <  monthEnd);

                int requests = await _context.DonationRequests
                    .AsNoTracking()
                    .CountAsync(r =>
                        !r.IsDeleted &&
                        r.DateOfCreattion >= monthStart &&
                        r.DateOfCreattion <  monthEnd);

                result.Labels.Add(monthStart.ToString("MMM yyyy"));
                result.Donations.Add(donations);
                result.Requests.Add(requests);
            }

            return result;
        }

        // ── GET /api/admin/analytics/hospitals-by-governorate ─
        public async Task<AdminHospitalsByGovernorateResult>
            GetHospitalsByGovernorateAsync()
        {
            var data = await _context.Hospitals
                .AsNoTracking()
                .Where(h => !h.IsDeleted && h.City != null)
                .GroupBy(h => new
                {
                    GovernorateId   = h.City.GovernorateId,
                    GovernorateName = h.City.Governorate.NameEn
                })
                .Select(g => new HospitalsByGovernorateDTO
                {
                    GovernorateId   = g.Key.GovernorateId,
                    GovernorateName = g.Key.GovernorateName,
                    HospitalCount   = g.Count(),
                    ActiveCount     = g.Count(h =>
                        h.Status == HospitalStatus.Active)
                })
                .OrderByDescending(g => g.HospitalCount)
                .ToListAsync();

            return new AdminHospitalsByGovernorateResult
            {
                Success = true,
                Message = "Hospitals by governorate retrieved successfully.",
                Value   = data
            };
        }

        // ── GET /api/admin/analytics/blood-type-demand ────────
        public async Task<AdminBloodTypeDemandResult>
            GetBloodTypeDemandAsync()
        {
            var data = await _context.DonationRequests
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .GroupBy(r => new
                {
                    r.BloodTypeId,
                    TypeName = r.BloodType.TypeName
                })
                .Select(g => new BloodTypeDemandDTO
                {
                    BloodTypeId      = g.Key.BloodTypeId,
                    BloodTypeName    = g.Key.TypeName,
                    RequestCount     = g.Count(),
                    FulfilledCount   = g.Count(r =>
                        r.Status == RequestStatus.Fulfilled),
                    FulfillmentRate  = g.Count() == 0 ? 0
                        : Math.Round(
                            (double)g.Count(r =>
                                r.Status == RequestStatus.Fulfilled)
                            / g.Count() * 100, 1)
                })
                .OrderByDescending(g => g.RequestCount)
                .ToListAsync();

            return new AdminBloodTypeDemandResult
            {
                Success = true,
                Message = "Blood type demand retrieved successfully.",
                Value   = data
            };
        }
    }
}
