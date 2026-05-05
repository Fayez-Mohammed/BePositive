// Base.Services/Interfaces/IAdminBloodRequestService.cs

using Base.Shared.DTOs.SystemAdminDTOs;

namespace Base.Services.Interfaces
{
    public interface IAdminBloodRequestService
    {
        Task<AdminBloodRequestStatsDTO>      GetStatsAsync();
        
        Task<AdminBloodRequestListResult>    GetAllRequestsAsync(
            string? hospitalId,
            string? bloodTypeId,
            string? status,
            string? urgencyLevel,
            string? search,
            int     page,
            int     limit);

        Task<AdminBloodRequestDetailResult>  GetRequestByIdAsync(string requestId);
        Task<AllHospitalsResult> GetHospitalsAsync();
     
    }
}
