// Base.Services/Interfaces/HospitalInterfaces/IHospitalProfileService.cs

using Base.Shared.DTOs.HospitalDTOs;

namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IHospitalProfileService
    {
        Task<HospitalProfileResult>       GetProfileAsync(string hospitalAdminUserId);
        Task<UpdateHospitalProfileResult> UpdateProfileAsync(
            string hospitalAdminUserId,
            UpdateHospitalProfileDTO dto);
    }
}
