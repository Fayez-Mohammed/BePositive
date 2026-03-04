using Base.DAL.Models.RequestModels;
using Base.Shared.DTOs;

namespace BaseAPI.Mappers;

public static class DonationRequestMapper
{
    public static DonationRequest ToDonationRequest(PostDonationRequestDto requestDto)
    {
        var request = new DonationRequest()
        {
           QuantityRequired = requestDto.QuantityRequired,
           HospitalId = requestDto.HospitalId,
           UrgencyLevel = requestDto.UrgencyLevel,
           Longitude = requestDto.Longitude,
           Latitude = requestDto.Latitude,
           Deadline = requestDto.Deadline,
           BloodTypeId = requestDto.BloodTypeId,
           Note = requestDto.Note ?? "No Note"
        };
        return request;
    }
}