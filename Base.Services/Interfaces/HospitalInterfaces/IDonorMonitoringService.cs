// Base.Services/Interfaces/HospitalInterfaces/IDonorMonitoringService.cs

using Base.Shared.DTOs.HospitalDTOs;

namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IDonorMonitoringService
    {
        Task<DonorStatsDTO> GetStatsAsync(string hospitalAdminUserId);
        Task<DonorListResult> GetDonorsAsync(string hospitalAdminUserId, GetDonorsQuery query);
        Task<DonorDetailResult> GetDonorByIdAsync(string hospitalAdminUserId, string donorId);
    }
}