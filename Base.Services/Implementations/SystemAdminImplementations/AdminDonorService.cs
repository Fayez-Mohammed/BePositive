// Base.Services/Implementations/AdminDonorService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations
{
    public class AdminDonorService : IAdminDonorService
    {
        private readonly AppDbContext _context;
        private const int CooldownDays = 56;

        public AdminDonorService(AppDbContext context)
        {
            _context = context;
        }

        private static bool IsEligible(
            DateOnly? lastDonationDate,
            bool isAvailable)
        {
            if (!isAvailable) return false;
            if (lastDonationDate == null) return true;
            return lastDonationDate.Value.AddDays(CooldownDays)
                   < DateOnly.FromDateTime(DateTime.UtcNow);
        }

        // ── GET /api/admin/donors/stats ───────────────────────
        public async Task<AdminDonorStatsDTO> GetStatsAsync()
        {
            var cutoff = DateOnly.FromDateTime(
                DateTime.UtcNow.AddDays(-CooldownDays));

            var thisMonthStart = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month, 1);

            var donors = await _context.Donors
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .Select(d => new
                {
                    d.LastDonationDate,
                    d.IsAvailableForDonation
                })
                .ToListAsync();

            int total      = donors.Count;
            int eligible   = donors.Count(d =>
                IsEligible(d.LastDonationDate, d.IsAvailableForDonation));
            int ineligible = total - eligible;

            int donatedThisMonth = await _context.DonationHistories
                .AsNoTracking()
                .Where(dh => dh.DonationDate >= thisMonthStart)
                .Select(dh => dh.DonorId)
                .Distinct()
                .CountAsync();

            return new AdminDonorStatsDTO
            {
                TotalDonors      = total,
                EligibleDonors   = eligible,
                IneligibleDonors = ineligible,
                DonatedThisMonth = donatedThisMonth
            };
        }

        // ── GET /api/admin/donors ─────────────────────────────
        public async Task<AdminDonorListResult> GetAllDonorsAsync(
            string? search,
            string? bloodTypeId,
            string? status,
            int     page,
            int     limit)
        {
            var cutoff = DateOnly.FromDateTime(
                DateTime.UtcNow.AddDays(-CooldownDays));

            var q = _context.Donors
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(bloodTypeId))
                q = q.Where(d => d.BloodTypeId == bloodTypeId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                q = q.Where(d =>
                    d.User.FullName.ToLower().Contains(s) ||
                    d.User.Email.ToLower().Contains(s) ||
                    (d.User.PhoneNumber != null &&
                     d.User.PhoneNumber.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "eligible")
                    q = q.Where(d =>
                        d.IsAvailableForDonation &&
                        (d.LastDonationDate == null ||
                         d.LastDonationDate.Value < cutoff));
                else if (status.ToLower() == "ineligible")
                    q = q.Where(d =>
                        !d.IsAvailableForDonation ||
                        (d.LastDonationDate != null &&
                         d.LastDonationDate.Value >= cutoff));
            }

            var total = await q.CountAsync();

            var rawDonors = await q
                .OrderBy(d => d.User.FullName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(d => new
                {
                    d.Id,
                    FullName             = d.User.FullName,
                    Email                = d.User.Email,
                    Phone                = d.User.PhoneNumber,
                    BloodTypeName        = d.BloodType.TypeName,
                    CityName             = d.City != null ? d.City.NameEn : null,
                    GovernorateName      = d.City != null &&
                                          d.City.Governorate != null
                                          ? d.City.Governorate.NameEn : null,
                    d.LastDonationDate,
                    d.IsAvailableForDonation,
                    d.DateOfCreattion
                })
                .ToListAsync();

            // Get total donations per donor
            var donorIds = rawDonors.Select(d => d.Id).ToList();
            var donationCounts = await _context.DonationHistories
                .AsNoTracking()
                .Where(dh => donorIds.Contains(dh.DonorId))
                .GroupBy(dh => dh.DonorId)
                .Select(g => new { DonorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DonorId, x => x.Count);

            var donors = rawDonors.Select(d => new AdminDonorSummaryDTO
            {
                DonorId          = d.Id,
                FullName         = d.FullName,
                Email            = d.Email,
                Phone            = d.Phone,
                BloodTypeName    = d.BloodTypeName,
                CityName         = d.CityName,
                GovernorateName  = d.GovernorateName,
                LastDonationDate = d.LastDonationDate,
                TotalDonations   = donationCounts
                    .TryGetValue(d.Id, out var c) ? c : 0,
                IsEligible       = IsEligible(
                    d.LastDonationDate, d.IsAvailableForDonation),
                IsAvailable      = d.IsAvailableForDonation,
                RegisteredAt     = d.DateOfCreattion
            }).ToList();

            return new AdminDonorListResult
            {
                Success    = true,
                Message    = "Donors retrieved successfully.",
                Total      = total,
                Page       = page,
                Limit      = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit),
                Value      = donors
            };
        }

        // ── GET /api/admin/donors/{id} ────────────────────────
        public async Task<AdminDonorDetailResult> GetDonorByIdAsync(
            string donorId)
        {
            var raw = await _context.Donors
                .AsNoTracking()
                .Where(d => d.Id == donorId && !d.IsDeleted)
                .Select(d => new
                {
                    d.Id,
                    FullName             = d.User.FullName,
                    Email                = d.User.Email,
                    Phone                = d.User.PhoneNumber,
                    d.BloodTypeId,
                    BloodTypeName        = d.BloodType.TypeName,
                    CityName             = d.City != null
                                          ? d.City.NameEn : null,
                    GovernorateName      = d.City != null &&
                                          d.City.Governorate != null
                                          ? d.City.Governorate.NameEn
                                          : null,
                    d.NationalId,
                    d.Gender,
                    d.BirthDate,
                    d.LastDonationDate,
                    d.IsAvailableForDonation,
                    d.Latitude,
                    d.Longitude,
                    d.DateOfCreattion
                })
                .FirstOrDefaultAsync();

            if (raw == null)
                return new AdminDonorDetailResult
                {
                    Success = false,
                    Message = "Donor not found."
                };

            var totalDonations = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync(dh => dh.DonorId == donorId);

            return new AdminDonorDetailResult
            {
                Success = true,
                Message = "Donor retrieved successfully.",
                Value   = new AdminDonorDetailDTO
                {
                    DonorId          = raw.Id,
                    FullName         = raw.FullName,
                    Email            = raw.Email,
                    Phone            = raw.Phone,
                    BloodTypeId      = raw.BloodTypeId,
                    BloodTypeName    = raw.BloodTypeName,
                    CityName         = raw.CityName,
                    GovernorateName  = raw.GovernorateName,
                    NationalId       = raw.NationalId,
                    Gender           = raw.Gender,
                    BirthDate        = raw.BirthDate,
                    LastDonationDate = raw.LastDonationDate,
                    TotalDonations   = totalDonations,
                    IsEligible       = IsEligible(
                        raw.LastDonationDate,
                        raw.IsAvailableForDonation),
                    IsAvailable      = raw.IsAvailableForDonation,
                    Latitude         = raw.Latitude,
                    Longitude        = raw.Longitude,
                    RegisteredAt     = raw.DateOfCreattion
                }
            };
        }
    }
}
