// Base.Services/Interfaces/HospitalInterfaces/IDashboardService.cs

using Base.Shared.DTOs.DashboardDTOs;

namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDTO>    GetStatsAsync(string hospitalAdminUserId);
        Task<RecentActivityResult> GetRecentActivityAsync(string hospitalAdminUserId, int limit = 4);
        Task<ActivityLogResult>    GetActivityLogAsync(string hospitalAdminUserId, ActivityLogQuery query);
    }
}
