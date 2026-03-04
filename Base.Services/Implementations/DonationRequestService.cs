using System.Linq.Expressions;
using Base.API.DTOs;
using Base.DAL.Models.RequestModels;
using Base.Repo.Interfaces;
using Base.Shared.DTOs;

namespace Base.Services.Implementations;

public class DonationRequestService(IUnitOfWork _unitOfWork)
{
    public async Task<IReadOnlyList<DonationRequestDto>> GetDonationRequest(QueryParameters queryParameters)
    {
        var requests = _unitOfWork.Repository<DonationRequest>();

        var list = await requests.ListAllAsync();
        
        var response  = list
            .Skip(queryParameters.PageNumber * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Select(a=> new DonationRequestDto
            {
                QuantityRequired = a.QuantityRequired,
                UrgencyLevel = a.UrgencyLevel,
                Longitude = a.Longitude,
                Latitude = a.Latitude,
                Deadline = a.Deadline
            })
            .ToList();
        return  response;
    }

    public async Task<bool> AddDonationRequest(DonationRequest request)
    {
        var requests =  _unitOfWork.Repository<DonationRequest>();

        try
        {
            await requests.AddAsync(request);

            if ((await _unitOfWork.CompleteAsync()) == 0)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<DonationRequestDto> GetDonationRequestById(string id)
    {
        var request = await _unitOfWork.Repository<DonationRequest>().GetByIdAsync(id);

        if (request == null)
            return new DonationRequestDto();

        var requestDto = new DonationRequestDto()
        {
            BloodTypeId = request.BloodTypeId,
            HospitalId = request.HospitalId,
            QuantityRequired = request.QuantityRequired,
            UrgencyLevel = request.UrgencyLevel,
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            Deadline = request.Deadline
        };
        
        return requestDto;
    }
    
    
}