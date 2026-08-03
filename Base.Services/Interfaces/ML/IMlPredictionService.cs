// Base.Services/Interfaces/IMlPredictionService.cs
using Base.Shared.DTOs.MlDTOs;

namespace Base.Services.Interfaces
{
    public interface IMlPredictionService
    {
        Task<List<DonorPredictionResultDTO>> GetDonorsPredictionsAsync(string? donorId);
        Task<MlHealthCheckResponseDTO?> CheckMlHealthAsync();
    }
}