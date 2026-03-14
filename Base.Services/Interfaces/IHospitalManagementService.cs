// Base.Services/Interfaces/IHospitalManagementService.cs

using Base.Shared.DTOs.HospitalDTOs;
using Base.Shared.DTOs.SystemAdminDTOs;
using Base.Shared.Enums;

namespace Base.Services.Interfaces
{
    public interface IHospitalManagementService
    {
        Task<HospitalListResult> GetAllHospitalsAsync(HospitalStatus? status, int page, int limit);
        Task<HospitalDetailResult> GetHospitalByIdAsync(string id);
        Task<HospitalStatusResult> UpdateStatusAsync(string id, HospitalStatus newStatus);
        Task<HospitalStatusResult> ActivateAsync(string id);
        Task<HospitalStatusResult> SuspendAsync(string id);
        Task<HospitalDeleteResult> DeleteAsync(string id, string deletedByUserId);
        Task<HospitalStatsResult> GetStatsAsync();
    }
}