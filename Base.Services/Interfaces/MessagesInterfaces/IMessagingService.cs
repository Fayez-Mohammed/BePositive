// Base.Services/Interfaces/HospitalInterfaces/IMessagingService.cs

using Base.Shared.DTOs.MessagingDTOs;

namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IMessagingService
    {
        // ── Conversations ─────────────────────────────────────────
        Task<ConversationListResult> GetConversationsAsync(
            string hospitalAdminUserId,
            GetConversationsQuery query);

        Task<ConversationResult> StartOrGetConversationAsync(
            string hospitalAdminUserId,
            string donorId);

        // ── Messages ──────────────────────────────────────────────
        Task<MessageListResult> GetMessagesAsync(
            string hospitalAdminUserId,
            string conversationId,
            GetMessagesQuery query);

        Task<MessageResult> SendMessageAsync(
            string hospitalAdminUserId,
            string conversationId,
            SendMessageDTO dto);

        Task<bool> DeleteMessageAsync(
            string hospitalAdminUserId,
            string messageId);

        Task<bool> MarkConversationReadAsync(
            string hospitalAdminUserId,
            string conversationId);

        // ── Unread count ──────────────────────────────────────────
        Task<UnreadCountResult> GetUnreadCountAsync(
            string hospitalAdminUserId);

        // ── Helpers used by SignalR Hub ───────────────────────────

        /// <summary>
        /// Resolve the conversationId from a donorId + hospitalId pair.
        /// Used by the Hub when a donor connects to join their group.
        /// </summary>
        Task<string?> GetConversationIdForDonorAsync(
            string donorId,
            string hospitalId);

        /// <summary>
        /// Used by Hub to send a donor message and return the saved DTO
        /// so the hub can broadcast it to the hospital group.
        /// </summary>
        Task<MessageResult> SendDonorMessageAsync(
            string donorUserId,
            string conversationId,
            string text);

        /// <summary>
        /// Mark messages as Read from the donor side.
        /// </summary>
        Task<bool> MarkConversationReadByDonorAsync(
            string donorUserId,
            string conversationId);
    }
}
