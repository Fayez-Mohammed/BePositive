// Base.API.Controllers/DonorControllers/DonorMessagingController.cs
//using Base.Services.Interfaces.DonorInterfaces;
using Base.Services.Interfaces.MessagesInterfaces;
using Base.Shared.DTOs.MessagingDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers.DonorControllers
{
    [Authorize(Roles = "Donor")] // مأمن فقط للـ Donors
    [ApiController]
    [Route("api/donor/conversations")]
    public class DonorMessagingController : ControllerBase
    {
        private readonly IDonorMessagingService _donorMessaging;

        public DonorMessagingController(IDonorMessagingService donorMessaging)
        {
            _donorMessaging = donorMessaging;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        [HttpGet]
        public async Task<IActionResult> GetConversations([FromQuery] GetConversationsQuery query)
        {
            var result = await _donorMessaging.GetConversationsAsync(UserId, query);
            return Ok(result);
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(string conversationId, [FromQuery] GetMessagesQuery query)
        {
            var result = await _donorMessaging.GetMessagesAsync(UserId, conversationId, query);
            return Ok(result);
        }
    }
}