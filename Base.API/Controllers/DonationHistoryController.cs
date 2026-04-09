using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Base.BLL.Services.Interfaces;
using Base.DAL.Models.RequestModels.DTOs;

namespace Base.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationHistoryController : ControllerBase
    {
        private readonly IDonationHistoryService _donationHistoryService;

        public DonationHistoryController(IDonationHistoryService donationHistoryService)
        {
            _donationHistoryService = donationHistoryService;
        }

        /// <summary>
        /// Gets all donation histories.
        /// </summary>
        /// <returns>A list of donation history DTOs.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonationHistoryDto>>> GetAllDonationHistories()
        {
            var donationHistories = await _donationHistoryService.GetAllDonationHistoriesAsync();
            return Ok(donationHistories);
        }

        /// <summary>
        /// Gets a specific donation history by ID.
        /// </summary>
        /// <param name="id">The ID of the donation history.</param>
        /// <returns>The donation history DTO.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DonationHistoryDto>> GetDonationHistoryById(string id)
        {
            var donationHistory = await _donationHistoryService.GetDonationHistoryByIdAsync(id);
            if (donationHistory == null)
            {
                return NotFound();
            }
            return Ok(donationHistory);
        }

        /// <summary>
        /// Gets donation histories for a specific donor.
        /// </summary>
        /// <param name="donorId">The ID of the donor.</param>
        /// <returns>A list of donation history DTOs for the specified donor.</returns>
        [HttpGet("donor/{donorId}")]
        public async Task<ActionResult<IEnumerable<DonationHistoryDto>>> GetDonationHistoriesByDonorId(string donorId)
        {
            var donationHistories = await _donationHistoryService.GetDonationHistoriesByDonorIdAsync(donorId);
            return Ok(donationHistories);
        }

        /// <summary>
        /// Creates a new donation history entry.
        /// </summary>
        /// <param name="createDto">The DTO containing data for the new donation history.</param>
        /// <returns>The created donation history DTO.</returns>
        [HttpPost]
        public async Task<ActionResult<DonationHistoryDto>> CreateDonationHistory([FromBody] CreateDonationHistoryDto createDto)
        {
            var donationHistory = await _donationHistoryService.CreateDonationHistoryAsync(createDto);
            return CreatedAtAction(nameof(GetDonationHistoryById), new { id = donationHistory.Id }, donationHistory);
        }

        /// <summary>
        /// Updates an existing donation history entry.
        /// </summary>
        /// <param name="id">The ID of the donation history to update.</param>
        /// <param name="updateDto">The DTO containing updated data for the donation history.</param>
        /// <returns>The updated donation history DTO.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<DonationHistoryDto>> UpdateDonationHistory(string id, [FromBody] UpdateDonationHistoryDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var updatedDonationHistory = await _donationHistoryService.UpdateDonationHistoryAsync(updateDto);
            if (updatedDonationHistory == null)
            {
                return NotFound();
            }
            return Ok(updatedDonationHistory);
        }

        /// <summary>
        /// Deletes a specific donation history entry.
        /// </summary>
        /// <param name="id">The ID of the donation history to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonationHistory(string id)
        {
            var result = await _donationHistoryService.DeleteDonationHistoryAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
