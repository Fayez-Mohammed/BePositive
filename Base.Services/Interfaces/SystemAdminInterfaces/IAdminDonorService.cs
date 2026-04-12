// Base.Services/Interfaces/IAdminDonorService.cs

using Base.Shared.DTOs.SystemAdminDTOs;

namespace Base.Services.Interfaces
{
    public interface IAdminDonorService
    {
        Task<AdminDonorStatsDTO>    GetStatsAsync();
        Task<AdminDonorListResult>  GetAllDonorsAsync(
            string?  search,
            string?  bloodTypeId,
            string?  status,
            int      page,
            int      limit);
        Task<AdminDonorDetailResult> GetDonorByIdAsync(string donorId);
    }
}
