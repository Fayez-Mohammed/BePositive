// Base.Services/Interfaces/IHospitalAuthService.cs

using Base.Shared.DTOs.HospitalDTOs;

namespace Base.Services.Interfaces
{
    public interface IHospitalAuthService
    {
        Task<ApiResponseDTO> RegisterAsync(HospitalRegisterRequest request);
    }
}