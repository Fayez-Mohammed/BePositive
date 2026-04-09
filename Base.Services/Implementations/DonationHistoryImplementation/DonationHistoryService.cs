 
using Base.BLL.Services.Interfaces;
using Base.DAL.Models.RequestModels;
using Base.DAL.Models.RequestModels.DTOs;

namespace Base.BLL.Services.Implementations
{
    public class DonationHistoryService : IDonationHistoryService
    {
        // In-memory store for demonstration purposes. In a real application, this would be a database context or repository.
        private static readonly List<DonationHistory> _donationHistories = new List<DonationHistory>();

        public DonationHistoryService()
        {
            // Seed some dummy data if needed
            if (!_donationHistories.Any())
            {
                _donationHistories.Add(new DonationHistory { Id = Guid.NewGuid().ToString(), DonorId = "donor123", DonationDate = DateTime.UtcNow.AddDays(-30), AmountML = 450, Report = "Healthy" });
                _donationHistories.Add(new DonationHistory { Id = Guid.NewGuid().ToString(), DonorId = "donor123", HospitalId = "hospital456", RequestId = "request789", DonationDate = DateTime.UtcNow.AddDays(-60), AmountML = 450, Report = "Healthy" });
                _donationHistories.Add(new DonationHistory { Id = Guid.NewGuid().ToString(), DonorId = "donor456", DonationDate = DateTime.UtcNow.AddDays(-10), AmountML = 450, Report = "Healthy" });
            }
        }

        public Task<IEnumerable<DonationHistoryDto>> GetAllDonationHistoriesAsync()
        {
            var dtos = _donationHistories.Select(dh => new DonationHistoryDto
            {
                Id = dh.Id,
                DonorId = dh.DonorId,
                HospitalId = dh.HospitalId,
                RequestId = dh.RequestId,
                DonationDate = dh.DonationDate,
                AmountML = dh.AmountML,
                Report = dh.Report
            });
            return Task.FromResult(dtos);
        }

        public Task<DonationHistoryDto?> GetDonationHistoryByIdAsync(string id)
        {
            var donationHistory = _donationHistories.FirstOrDefault(dh => dh.Id == id);
            if (donationHistory == null)
            {
                return Task.FromResult<DonationHistoryDto?>(null);
            }

            var dto = new DonationHistoryDto
            {
                Id = donationHistory.Id,
                DonorId = donationHistory.DonorId,
                HospitalId = donationHistory.HospitalId,
                RequestId = donationHistory.RequestId,
                DonationDate = donationHistory.DonationDate,
                AmountML = donationHistory.AmountML,
                Report = donationHistory.Report
            };
            return Task.FromResult<DonationHistoryDto?>(dto);
        }

        public Task<IEnumerable<DonationHistoryDto>> GetDonationHistoriesByDonorIdAsync(string donorId)
        {
            var dtos = _donationHistories.Where(dh => dh.DonorId == donorId).Select(dh => new DonationHistoryDto
            {
                Id = dh.Id,
                DonorId = dh.DonorId,
                HospitalId = dh.HospitalId,
                RequestId = dh.RequestId,
                DonationDate = dh.DonationDate,
                AmountML = dh.AmountML,
                Report = dh.Report
            });
            return Task.FromResult(dtos);
        }

        public Task<DonationHistoryDto> CreateDonationHistoryAsync(CreateDonationHistoryDto donationHistoryDto)
        {
            var newDonationHistory = new DonationHistory
            {
                Id = Guid.NewGuid().ToString(),
                DonorId = donationHistoryDto.DonorId,
                HospitalId = donationHistoryDto.HospitalId,
                RequestId = donationHistoryDto.RequestId,
                DonationDate = donationHistoryDto.DonationDate,
                AmountML = donationHistoryDto.AmountML,
                Report = donationHistoryDto.Report
            };
            _donationHistories.Add(newDonationHistory);

            var dto = new DonationHistoryDto
            {
                Id = newDonationHistory.Id,
                DonorId = newDonationHistory.DonorId,
                HospitalId = newDonationHistory.HospitalId,
                RequestId = newDonationHistory.RequestId,
                DonationDate = newDonationHistory.DonationDate,
                AmountML = newDonationHistory.AmountML,
                Report = newDonationHistory.Report
            };
            return Task.FromResult(dto);
        }

        public Task<DonationHistoryDto?> UpdateDonationHistoryAsync(UpdateDonationHistoryDto donationHistoryDto)
        {
            var existingDonationHistory = _donationHistories.FirstOrDefault(dh => dh.Id == donationHistoryDto.Id);
            if (existingDonationHistory == null)
            {
                return Task.FromResult<DonationHistoryDto?>(null);
            }

            existingDonationHistory.HospitalId = donationHistoryDto.HospitalId;
            existingDonationHistory.RequestId = donationHistoryDto.RequestId;
            existingDonationHistory.DonationDate = donationHistoryDto.DonationDate;
            existingDonationHistory.AmountML = donationHistoryDto.AmountML;
            existingDonationHistory.Report = donationHistoryDto.Report;

            var dto = new DonationHistoryDto
            {
                Id = existingDonationHistory.Id,
                DonorId = existingDonationHistory.DonorId,
                HospitalId = existingDonationHistory.HospitalId,
                RequestId = existingDonationHistory.RequestId,
                DonationDate = existingDonationHistory.DonationDate,
                AmountML = existingDonationHistory.AmountML,
                Report = existingDonationHistory.Report
            };
            return Task.FromResult<DonationHistoryDto?>(dto);
        }

        public Task<bool> DeleteDonationHistoryAsync(string id)
        {
            var donationHistoryToRemove = _donationHistories.FirstOrDefault(dh => dh.Id == id);
            if (donationHistoryToRemove == null)
            {
                return Task.FromResult(false);
            }

            _donationHistories.Remove(donationHistoryToRemove);
            return Task.FromResult(true);
        }
    }
}
