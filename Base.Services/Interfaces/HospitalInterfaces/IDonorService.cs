using Base.Shared.DTOs.DonorDTOs;
using System.Threading.Tasks;

namespace Base.Services.Interfaces
{
    public interface IDonorService
    {
        Task<AcceptRequestResultDTO> AcceptBloodRequestAsync(string donorUserId, AcceptRequestDTO dto);
        Task<bool> CompleteDonorProfileAsync(string userId, CompleteProfileDTO dto);
        Task<bool> UpdateFcmTokenAsync(string userId, UpdateFcmDTO dto);
        Task<DonorNearbyRequestListResult> GetNearbyRequestsAsync(string donorUserId, NearbyRequestQueryDTO query);
    }
}