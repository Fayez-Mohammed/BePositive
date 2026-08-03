// Base.Services/Interfaces/DonorInterfaces/IDonorMessagingService.cs
using Base.Shared.DTOs.MessagingDTOs;

namespace Base.Services.Interfaces.MessagesInterfaces
{
    public interface IDonorMessagingService
    {
        Task<DonorConversationListResult> GetConversationsAsync(string donorUserId, GetConversationsQuery query);
        Task<DonorMessageListResult> GetMessagesAsync(string donorUserId, string conversationId, GetMessagesQuery query);
    }
}