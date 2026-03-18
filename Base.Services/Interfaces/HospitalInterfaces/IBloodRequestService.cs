// Base.Services/Interfaces/IBloodRequestService.cs

using Base.Shared.DTOs.HospitalDTOs;

namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IBloodRequestService
    {

        // ── existing ──────────────────────────────────────────────
        Task<BloodRequestResponseDTO> CreateRequestAsync(
            string hospitalAdminUserId,
            CreateBloodRequestDTO dto);

        // ── new ───────────────────────────────────────────────────
        Task<BloodRequestListResult> GetAllRequestsAsync(
            string hospitalAdminUserId,
            GetBloodRequestsQuery query);

        Task<BloodRequestDetailResult> GetRequestByIdAsync(
            string hospitalAdminUserId,
            string requestId);

        Task<BloodRequestResponseDTO> UpdateRequestAsync(
            string hospitalAdminUserId,
            string requestId,
            UpdateBloodRequestDTO dto);

        Task<bool> CancelRequestAsync(
            string hospitalAdminUserId,
            string requestId);

       
        
    }
}
