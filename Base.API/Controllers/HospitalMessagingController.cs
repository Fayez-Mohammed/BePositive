// Base.API/Controllers/Hospital/HospitalMessagingController.cs

using Base.API.DTOs;
using Base.API.Hubs;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.MessagingDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Base.API.Controllers.Hospital
{
    [ApiController]
    [Route("api/hospital")]
    [Authorize(Roles = nameof(UserTypes.HospitalAdmin))]
    public class HospitalMessagingController : ControllerBase
    {
        private readonly IMessagingService _service;

        public HospitalMessagingController(IMessagingService service)
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

        // ── GET /api/hospital/conversations ──────────────────────
        /// <summary>List all conversations for this hospital.</summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(
            [FromQuery] GetConversationsQuery query)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetConversationsAsync(userId, query);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401, Message = ex.Message
                });
            }
        }

        // ── POST /api/hospital/conversations ─────────────────────
        /// <summary>
        /// Start (or fetch) a conversation with a donor.
        /// Called from Active Requests / Donors Monitoring when clicking "Message".
        /// </summary>
        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation(
            [FromBody] StartConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400, Message = "Invalid request."
                });

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.StartOrGetConversationAsync(
                    userId, dto.DonorId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401, Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404, Message = ex.Message
                });
            }
        }

        // ── GET /api/hospital/conversations/:id/messages ──────────
        /// <summary>Paginated message history. Newest first, scroll up to load more.</summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(
            string conversationId,
            [FromQuery] GetMessagesQuery query)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetMessagesAsync(
                    userId, conversationId, query);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401, Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404, Message = ex.Message
                });
            }
        }

        // ── POST /api/hospital/conversations/:id/messages ─────────
        /// <summary>
        /// Send a message via REST (fallback if SignalR is unavailable).
        /// Prefer using the SignalR hub SendMessage method for real-time delivery.
        /// </summary>
        /// 
        [HttpPost("conversations/{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(
    string conversationId,
    [FromBody] SendMessageDTO dto,
    [FromServices] IHubContext<MessagingHub> hubContext)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.SendMessageAsync(userId, conversationId, dto);

                if (result.Success && result.Value != null)
                {
                    // Broadcast to all connections in this conversation group
                    await hubContext.Clients
                        .Group($"conv_{conversationId}")
                        .SendAsync("message:new", new
                        {
                            messagedto = result.Value,
                            conversationdto = (object?)null
                        });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO { StatusCode = 401, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO { StatusCode = 404, Message = ex.Message });
            }
        }
        //[HttpPost("conversations/{conversationId}/messages")]
        //public async Task<IActionResult> SendMessage(
        //    string conversationId,
        //    [FromBody] SendMessageDTO dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(new ApiErrorResponseDTO
        //        {
        //            StatusCode = 400, Message = "Invalid request."
        //        });

        //    var userId = GetUserId();
        //    if (string.IsNullOrEmpty(userId)) return Unauthorized401();

        //    try
        //    {
        //        var result = await _service.SendMessageAsync(
        //            userId, conversationId, dto);
        //        return Ok(result);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new ApiErrorResponseDTO
        //        {
        //            StatusCode = 401, Message = ex.Message
        //        });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new ApiErrorResponseDTO
        //        {
        //            StatusCode = 404, Message = ex.Message
        //        });
        //    }
        //}

        // ── DELETE /api/hospital/messages/:messageId ──────────────
        /// <summary>Soft-delete a message sent by this hospital admin.</summary>
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                await _service.DeleteMessageAsync(userId, messageId);
                return Ok(new { success = true, message = "Message deleted." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401, Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404, Message = ex.Message
                });
            }
        }

        // ── POST /api/hospital/conversations/:id/read ─────────────
        /// <summary>Mark all donor messages in this conversation as read.</summary>
        [HttpPost("conversations/{conversationId}/read")]
        public async Task<IActionResult> MarkRead(string conversationId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                await _service.MarkConversationReadAsync(userId, conversationId);
                return Ok(new { success = true, message = "Marked as read." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401, Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404, Message = ex.Message
                });
            }
        }

        // ── GET /api/hospital/messages/unread-count ───────────────
        /// <summary>Total unread messages count for the sidebar badge.</summary>
        [HttpGet("messages/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetUnreadCountAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401, Message = ex.Message
                });
            }
        }
    }
}
