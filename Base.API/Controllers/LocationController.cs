// Base.API/Controllers/LocationController.cs
using Base.DAL.Contexts;
using Base.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all governorates
        /// </summary>
        [HttpGet("governorates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGovernorates()
        {
            var governorates = await _context.Governorates
                .OrderBy(g => g.NameEn)
                .Select(g => new
                {
                    g.Id,
                    g.NameEn,
                    g.NameAr
                })
                .ToListAsync();

            return Ok(governorates);
        }

        /// <summary>
        /// Get cities by governorate
        /// </summary>
        [HttpGet("governorates/{governorateId}/cities")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCitiesByGovernorate(string governorateId)
        {
            var exists = await _context.Governorates.AnyAsync(g => g.Id == governorateId);
            if (!exists)
                return StatusCode(404, new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = "Governorate not found."
                });

            var cities = await _context.Cities
                .Where(c => c.GovernorateId == governorateId)
                .OrderBy(c => c.NameEn)
                .Select(c => new
                {
                    c.Id,
                    c.NameEn,
                    c.NameAr
                })
                .ToListAsync();

            return Ok(cities);
        }

        /// <summary>
        /// Autocomplete — search cities by name (Arabic or English)
        /// Used for frontend city search input
        /// </summary>
        [HttpGet("cities/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCities(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Ok(new List<object>());

            // Cap limit to prevent abuse
            if (limit > 50) limit = 50;

            var cities = await _context.Cities
                .Where(c =>
                    c.NameEn.Contains(query) ||
                    c.NameAr.Contains(query))
                .OrderBy(c => c.NameEn)
                .Take(limit)
                .Select(c => new
                {
                    c.Id,
                    c.NameEn,
                    c.NameAr,
                    Governorate = new
                    {
                        c.Governorate.Id,
                        c.Governorate.NameEn,
                        c.Governorate.NameAr
                    }
                })
                .ToListAsync();

            return Ok(cities);
        }
    }
}