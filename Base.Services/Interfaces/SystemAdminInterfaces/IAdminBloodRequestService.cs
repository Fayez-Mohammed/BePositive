// Base.Services/Interfaces/IAdminBloodRequestService.cs

using Base.Shared.DTOs.AdminDTOs;
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
        // ── New ───────────────────────────────────────────────

        /// <summary>
        /// Get all requests for a specific hospital.
        /// GET /api/admin/requests/hospital/{hospitalId}
        /// </summary>
        Task<AdminRequestListResult> GetRequestsByHospitalAsync(
            string hospitalId,
            string? status,
            string? urgencyLevel,
            int page,
            int limit);

        /// <summary>
        /// Update request status (Cancelled or Expired only).
        /// PATCH /api/admin/requests/{id}/status
        /// </summary>
        Task<AdminRequestDetailResult> UpdateRequestStatusAsync(
            string requestId,
            UpdateRequestStatusDTO dto);

        /// <summary>
        /// Soft delete a request.
        /// DELETE /api/admin/requests/{id}
        /// </summary>
        Task<AdminDeleteResult> DeleteRequestAsync(string requestId);

    }
}
