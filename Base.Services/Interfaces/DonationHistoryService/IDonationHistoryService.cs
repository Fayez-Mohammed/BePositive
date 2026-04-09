 
using Base.DAL.Models.RequestModels.DTOs;

namespace Base.BLL.Services.Interfaces
{
    public interface IDonationHistoryService
    {
        Task<IEnumerable<DonationHistoryDto>> GetAllDonationHistoriesAsync();
        Task<DonationHistoryDto?> GetDonationHistoryByIdAsync(string id);
        Task<IEnumerable<DonationHistoryDto>> GetDonationHistoriesByDonorIdAsync(string donorId);
        Task<DonationHistoryDto> CreateDonationHistoryAsync(CreateDonationHistoryDto donationHistoryDto);
        Task<DonationHistoryDto?> UpdateDonationHistoryAsync(UpdateDonationHistoryDto donationHistoryDto);
        Task<bool> DeleteDonationHistoryAsync(string id);
    }
}