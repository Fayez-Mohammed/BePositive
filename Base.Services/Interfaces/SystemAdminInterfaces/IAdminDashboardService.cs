// Base.Services/Interfaces/IAdminDashboardService.cs

using Base.Shared.DTOs.SystemAdminDTOs;

namespace Base.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardStatsDTO>      GetStatsAsync();
        Task<RecentRegistrationsResult>   GetRecentRegistrationsAsync(int limit = 5);
        Task<AdminActivityChartDTO>       GetActivityChartAsync();
    }
}
