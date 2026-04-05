// Base.Services/Implementations/HospitalImplementations/DonorMonitoringService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class DonorMonitoringService : IDonorMonitoringService
    {
        private readonly AppDbContext _context;
        private const int CooldownDays = 56;
        private const int RecentDays = 30;

        public DonorMonitoringService(AppDbContext context)
        {
            _context = context;
        }

        // ── Helper: get hospital from admin user ──────────────────
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

        // ── Helper: get unique donor IDs for this hospital ────────
        private async Task<List<string>> GetHospitalDonorIdsAsync(string hospitalId)
        {
            return await _context.RequestResponses
                .AsNoTracking()
                .Where(rr =>
                    rr.Request.HospitalId == hospitalId &&
                    !rr.Request.IsDeleted)
                .Select(rr => rr.DonorId)
                .Distinct()
                .ToListAsync();
        }

        // ── Helper: check eligibility ─────────────────────────────
        // A donor is eligible only if BOTH conditions are true:
        //   1. IsAvailableForDonation = true (donor manually set)
        //   2. LastDonationDate is null OR more than 56 days ago (medical rule)
        private static bool IsEligible(
            DateOnly? lastDonationDate,
            bool isAvailableForDonation)
        {
            if (!isAvailableForDonation) return false;
            if (lastDonationDate == null) return true;
            return lastDonationDate.Value.AddDays(CooldownDays)
                   < DateOnly.FromDateTime(DateTime.UtcNow);
        }

        // ── Stats ─────────────────────────────────────────────────
        public async Task<DonorStatsDTO> GetStatsAsync(string hospitalAdminUserId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);
            var donorIds = await GetHospitalDonorIdsAsync(hospitalId);

            if (!donorIds.Any())
                return new DonorStatsDTO
                {
                    EligibleDonors = 0,
                    RecentlyDonated = 0,
                    CurrentlyIneligible = 0
                };

            var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-CooldownDays));
            var recentCutoff = DateTime.UtcNow.AddDays(-RecentDays);

            var donors = await _context.Donors
                .AsNoTracking()
                .Where(d => donorIds.Contains(d.Id) && !d.IsDeleted)
                .Select(d => new
                {
                    d.Id,
                    d.LastDonationDate,
                    d.IsAvailableForDonation
                })
                .ToListAsync();

            // Eligible = available AND not in cooldown
            var eligibleCount = donors.Count(d =>
                d.IsAvailableForDonation &&
                (d.LastDonationDate == null ||
                 d.LastDonationDate.Value < cutoffDate));

            // Ineligible = not available OR within cooldown period
            var ineligibleCount = donors.Count(d =>
                !d.IsAvailableForDonation ||
                (d.LastDonationDate != null &&
                 d.LastDonationDate.Value >= cutoffDate));

            // Recently donated to THIS hospital in last 30 days
            var recentCount = await _context.DonationHistories
                .AsNoTracking()
                .Where(dh =>
                    dh.HospitalId == hospitalId &&
                    donorIds.Contains(dh.DonorId) &&
                    dh.DonationDate >= recentCutoff)
                .Select(dh => dh.DonorId)
                .Distinct()
                .CountAsync();

            return new DonorStatsDTO
            {
                EligibleDonors = eligibleCount,
                RecentlyDonated = recentCount,
                CurrentlyIneligible = ineligibleCount
            };
        }

        // ── Get Donors List ───────────────────────────────────────
        public async Task<DonorListResult> GetDonorsAsync(
            string hospitalAdminUserId,
            GetDonorsQuery query)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);
            var donorIds = await GetHospitalDonorIdsAsync(hospitalId);

            if (!donorIds.Any())
                return new DonorListResult
                {
                    Success = true,
                    Message = "No donors found.",
                    Total = 0,
                    Page = query.Page,
                    Limit = query.Limit,
                    TotalPages = 0,
                    Value = new List<DonorSummaryDTO>()
                };

            var cutoffDate = DateOnly.FromDateTime(
                DateTime.UtcNow.AddDays(-CooldownDays));

            // ── Base query ────────────────────────────────────────
            var q = _context.Donors
                .AsNoTracking()
                .Where(d => donorIds.Contains(d.Id) && !d.IsDeleted)
                .AsQueryable();

            // ── Filters ───────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(query.BloodTypeId))
                q = q.Where(d => d.BloodTypeId == query.BloodTypeId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(d =>
                    d.User.FullName.ToLower().Contains(search) ||
                    (d.User.PhoneNumber != null &&
                     d.User.PhoneNumber.Contains(search)) ||
                    d.BloodType.TypeName.ToLower().Contains(search));
            }

            // ── Eligibility filter — uses both conditions ─────────
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (query.Status.ToLower() == "eligible")
                    q = q.Where(d =>
                        d.IsAvailableForDonation &&
                        (d.LastDonationDate == null ||
                         d.LastDonationDate.Value < cutoffDate));

                else if (query.Status.ToLower() == "ineligible")
                    q = q.Where(d =>
                        !d.IsAvailableForDonation ||
                        (d.LastDonationDate != null &&
                         d.LastDonationDate.Value >= cutoffDate));
            }

            var total = await q.CountAsync();

            // ── Project ───────────────────────────────────────────
            var rawDonors = await q
                .OrderBy(d => d.User.FullName)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(d => new
                {
                    d.Id,
                    FullName = d.User.FullName,
                    Phone = d.User.PhoneNumber,
                    BloodTypeName = d.BloodType.TypeName,
                    d.LastDonationDate,
                    d.IsAvailableForDonation
                })
                .ToListAsync();

            // ── Total donations per donor for THIS hospital ───────
            var donorIdsPage = rawDonors.Select(d => d.Id).ToList();

            var donationCounts = await _context.DonationHistories
                .AsNoTracking()
                .Where(dh =>
                    dh.HospitalId == hospitalId &&
                    donorIdsPage.Contains(dh.DonorId))
                .GroupBy(dh => dh.DonorId)
                .Select(g => new { DonorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DonorId, x => x.Count);

            var donors = rawDonors.Select(d => new DonorSummaryDTO
            {
                DonorId = d.Id,
                FullName = d.FullName,
                Phone = d.Phone,
                BloodTypeName = d.BloodTypeName,
                LastDonationDate = d.LastDonationDate,
                TotalDonations = donationCounts.TryGetValue(d.Id, out var c) ? c : 0,
                IsEligible = IsEligible(d.LastDonationDate, d.IsAvailableForDonation)
            }).ToList();

            return new DonorListResult
            {
                Success = true,
                Message = "Donors retrieved successfully.",
                Total = total,
                Page = query.Page,
                Limit = query.Limit,
                TotalPages = (int)Math.Ceiling((double)total / query.Limit),
                Value = donors
            };
        }

        // ── Get Donor Detail ──────────────────────────────────────
        public async Task<DonorDetailResult> GetDonorByIdAsync(
            string hospitalAdminUserId,
            string donorId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);
            var donorIds = await GetHospitalDonorIdsAsync(hospitalId);

            // Verify this donor responded to this hospital
            if (!donorIds.Contains(donorId))
                return new DonorDetailResult
                {
                    Success = false,
                    Message = "Donor not found."
                };

            var raw = await _context.Donors
                .AsNoTracking()
                .Where(d => d.Id == donorId && !d.IsDeleted)
                .Select(d => new
                {
                    d.Id,
                    FullName = d.User.FullName,
                    Email = d.User.Email,
                    Phone = d.User.PhoneNumber,
                    BloodTypeName = d.BloodType.TypeName,
                    CityName = d.City != null ? d.City.NameEn : null,
                    d.LastDonationDate,
                    d.IsAvailableForDonation
                })
                .FirstOrDefaultAsync();

            if (raw == null)
                return new DonorDetailResult
                {
                    Success = false,
                    Message = "Donor not found."
                };

            // Total donations to THIS hospital only
            var totalDonations = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync(dh =>
                    dh.DonorId == donorId &&
                    dh.HospitalId == hospitalId);

            return new DonorDetailResult
            {
                Success = true,
                Message = "Donor retrieved successfully.",
                Value = new DonorDetailDTO
                {
                    DonorId = raw.Id,
                    FullName = raw.FullName,
                    BloodTypeName = raw.BloodTypeName,
                    CityName = raw.CityName,
                    Phone = raw.Phone,
                    Email = raw.Email,
                    LastDonationDate = raw.LastDonationDate,
                    TotalDonations = totalDonations,
                    IsEligible = IsEligible(raw.LastDonationDate, raw.IsAvailableForDonation)
                }
            };
        }
    }
}