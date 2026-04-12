// Base.Services/Implementations/AdminDashboardService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        // ── GET /api/admin/dashboard/stats ────────────────────
        public async Task<AdminDashboardStatsDTO> GetStatsAsync()
        {
            var totalHospitals    = await _context.Hospitals
                .AsNoTracking()
                .CountAsync(h => !h.IsDeleted);

            var activeHospitals   = await _context.Hospitals
                .AsNoTracking()
                .CountAsync(h => !h.IsDeleted &&
                    h.Status == HospitalStatus.Active);

            var pendingReview     = await _context.Hospitals
                .AsNoTracking()
                .CountAsync(h => !h.IsDeleted &&
                    h.Status == HospitalStatus.UnderReview);

            var suspendedHospitals = await _context.Hospitals
                .AsNoTracking()
                .CountAsync(h => !h.IsDeleted &&
                    h.Status == HospitalStatus.Suspended);

            var totalDonors = await _context.Donors
                .AsNoTracking()
                .CountAsync(d => !d.IsDeleted);

            var totalDonations = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync();

            var totalRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r => !r.IsDeleted);

            return new AdminDashboardStatsDTO
            {
                TotalHospitals      = totalHospitals,
                ActiveHospitals     = activeHospitals,
                PendingReview       = pendingReview,
                SuspendedHospitals  = suspendedHospitals,
                TotalDonors         = totalDonors,
                TotalDonations      = totalDonations,
                TotalRequests       = totalRequests
            };
        }

        // ── GET /api/admin/dashboard/recent-registrations ─────
        public async Task<RecentRegistrationsResult> GetRecentRegistrationsAsync(
            int limit = 5)
        {
            var hospitals = await _context.Hospitals
                .AsNoTracking()
                .Where(h => !h.IsDeleted)
                .OrderByDescending(h => h.DateOfCreattion)
                .Take(limit)
                .Select(h => new RecentRegistrationDTO
                {
                    Id              = h.Id,
                    Name            = h.Name,
                    Email           = h.Email ?? "",
                    Phone           = h.Phone,
                    LicenseNumber   = h.LicenseNumber,
                    Status          = h.Status.ToString(),
                    CityName        = h.City != null ? h.City.NameEn : null,
                    GovernorateName = h.City != null && h.City.Governorate != null
                                      ? h.City.Governorate.NameEn : null,
                    RegisteredAt    = h.DateOfCreattion
                })
                .ToListAsync();

            return new RecentRegistrationsResult
            {
                Success = true,
                Message = "Recent registrations retrieved successfully.",
                Value   = hospitals
            };
        }

        // ── GET /api/admin/dashboard/activity-chart ───────────
        public async Task<AdminActivityChartDTO> GetActivityChartAsync()
        {
            var result = new AdminActivityChartDTO();

            // Last 12 months
            for (int i = 11; i >= 0; i--)
            {
                var monthStart = new DateTime(
                    DateTime.UtcNow.Year,
                    DateTime.UtcNow.Month, 1)
                    .AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);

                int registrations = await _context.Hospitals
                    .AsNoTracking()
                    .CountAsync(h =>
                        !h.IsDeleted &&
                        h.DateOfCreattion >= monthStart &&
                        h.DateOfCreattion <  monthEnd);

                int donations = await _context.DonationHistories
                    .AsNoTracking()
                    .CountAsync(dh =>
                        dh.DonationDate >= monthStart &&
                        dh.DonationDate <  monthEnd);

                result.Labels.Add(monthStart.ToString("MMM"));
                result.Registrations.Add(registrations);
                result.Donations.Add(donations);
            }

            return result;
        }
    }
}
