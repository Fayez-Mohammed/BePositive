// Base.API/Controllers/Hospital/BloodInventoryController.cs

using Base.API.DTOs;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.InventoryDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers.Hospital
{
    [ApiController]
    [Route("api/hospital/inventory")]
    [Authorize(Roles = nameof(UserTypes.HospitalAdmin))]
    public class BloodInventoryController : ControllerBase
    {
        private readonly IBloodInventoryService _service;

        public BloodInventoryController(IBloodInventoryService service)
        {
            _service = service;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private IActionResult Unauthorized401() =>
            Unauthorized(new ApiErrorResponseDTO
            {
                StatusCode = 401,
                Message    = "Unauthorized."
            });

        // ── GET /api/hospital/inventory ───────────────────────────
        /// <summary>
        /// Get current blood inventory overview for all blood types.
        /// Example: GET /api/hospital/inventory
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInventory()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetInventoryAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }

        // ── GET /api/hospital/inventory/{bloodTypeId} ─────────────
        /// <summary>
        /// Get inventory detail for a specific blood type including all batches.
        /// Example: GET /api/hospital/inventory/bt-apos
        /// </summary>
        [HttpGet("{bloodTypeId}")]
        public async Task<IActionResult> GetInventoryByBloodType(
            [FromRoute] string bloodTypeId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service
                    .GetInventoryByBloodTypeAsync(userId, bloodTypeId);

                if (!result.Success)
                    return NotFound(new ApiErrorResponseDTO
                    {
                        StatusCode = 404,
                        Message    = result.Message
                    });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }

        // ── POST /api/hospital/inventory/batches/add ──────────────
        /// <summary>
        /// Add a new blood batch to inventory.
        /// Example: POST /api/hospital/inventory/batches/add
        /// Body: { bloodTypeId, units, collectionDate, expiryDate, notes }
        /// </summary>
        [HttpPost("batches/add")]
        public async Task<IActionResult> AddBatch(
            [FromBody] AddBatchDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message    = "Invalid request data."
                });

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.AddBatchAsync(userId, dto);
                return StatusCode(201, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message    = ex.Message
                });
            }
        }

        // ── POST /api/hospital/inventory/withdraw ─────────────────
        /// <summary>
        /// Withdraw blood units from inventory (FIFO — oldest batch first).
        /// Example: POST /api/hospital/inventory/withdraw
        /// Body: { bloodTypeId, units, requestId?, notes? }
        /// </summary>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(
            [FromBody] WithdrawDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message    = "Invalid request data."
                });

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.WithdrawAsync(userId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message    = ex.Message
                });
            }
        }

        // ── GET /api/hospital/inventory/expiring-soon ─────────────
        /// <summary>
        /// Get blood batches expiring within the next N days (default 7).
        /// Example: GET /api/hospital/inventory/expiring-soon?days=7
        /// </summary>
        [HttpGet("expiring-soon")]
        public async Task<IActionResult> GetExpiringSoon(
            [FromQuery] int days = 7)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetExpiringSoonAsync(userId, days);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }

        // ── GET /api/hospital/inventory/transactions ──────────────
        /// <summary>
        /// Get inventory transaction history (audit log).
        /// Example: GET /api/hospital/inventory/transactions?page=1&limit=10
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] int page  = 1,
            [FromQuery] int limit = 10)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service
                    .GetTransactionsAsync(userId, page, limit);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }

        // ── GET /api/hospital/inventory/compatible/{bloodTypeId} ──
        /// <summary>
        /// Check available inventory for a blood type — exact match first,
        /// then compatible types as fallback.
        /// Example: GET /api/hospital/inventory/compatible/bt-apos
        /// </summary>
        [HttpGet("compatible/{bloodTypeId}")]
        public async Task<IActionResult> GetCompatibleInventory(
            [FromRoute] string bloodTypeId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service
                    .GetCompatibleInventoryAsync(userId, bloodTypeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }
    }
}
