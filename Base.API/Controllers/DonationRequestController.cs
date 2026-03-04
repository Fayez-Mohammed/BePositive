using Base.API.DTOs;
using Base.DAL.Models.RequestModels;
using Base.Services.Implementations;
using Base.Shared.DTOs;
using BaseAPI.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationRequestController(DonationRequestService _service) : Controller
{
    [HttpGet("requests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDonationRequests([FromQuery] QueryParameters queryParameters)
    {
        if (queryParameters.PageNumber < 1 || queryParameters.PageSize < 1)
            return BadRequest();
        try
        {
            return Ok(await _service.GetDonationRequest(queryParameters));
        }
        catch (Exception ex)
        {
            return StatusCode(500,"Internal server error");
        }
    }

    [Authorize]
    [HttpPost("requests")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostDonationRequest([FromBody] PostDonationRequestDto donationRequest)
    {
        if (string.IsNullOrEmpty(donationRequest.BloodTypeId))
            return BadRequest();
        
        if (string.IsNullOrEmpty(donationRequest.HospitalId))
           return BadRequest();
        var request = DonationRequestMapper.ToDonationRequest(donationRequest);
        
        
        try
        {
          var res =  await  _service.AddDonationRequest(request);

          if (res)
              return Created();
          return BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(500,"Internal server error");
        }
    }

    [HttpGet("requests/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDonationRequest(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrEmpty(id))
            return BadRequest();
        try
        {
            var request = await _service.GetDonationRequestById(id);
            
            if (string.IsNullOrEmpty(request.HospitalId))
                return NotFound();
            return Ok(request);
        }
        catch 
        {
            return StatusCode(500,"Internal server error");
        }
        
    }
}