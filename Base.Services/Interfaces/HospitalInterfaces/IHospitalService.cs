using Base.Shared.DTOs.HospitalDTOs;
using System.Threading.Tasks;
namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IHospitalService
    {
        Task<EligibleDonorsCountResponseDTO> GetNearbyEligibleDonorsCountAsync(
            string hospitalAdminUserId,
            GetEligibleDonorsCountDTO dto);
    }
}