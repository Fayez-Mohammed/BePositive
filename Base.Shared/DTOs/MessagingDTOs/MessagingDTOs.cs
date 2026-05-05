// Base.Shared/DTOs/MessagingDTOs/MessagingDTOs.cs

using System.ComponentModel.DataAnnotations;

namespace Base.Shared.DTOs.MessagingDTOs
{
    // ═══════════════════════════════════════════════════
    // CONVERSATION DTOs
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Returned in the conversations list and after starting a conversation.
    /// All field names serialize as lowercase (LowerCaseNamingPolicy).
    /// </summary>
    public class ConversationDTO
    {
        public string   ConversationId   { get; set; } = null!;
        public string   DonorId          { get; set; } = null!;
        public string   FullName         { get; set; } = null!;
        public string?  BloodTypeName    { get; set; }
        public string?  Phone            { get; set; }
        public string?  City             { get; set; }
        public bool     Online           { get; set; } // from SignalR presence
        public string?  LastMessage      { get; set; }
        public DateTime? LastMessageAt   { get; set; }
        public int      UnreadCount      { get; set; }
    }

    /// <summary>Query parameters for GET /hospital/conversations</summary>
    public class GetConversationsQuery
    {
        public string? Search   { get; set; }
        public int     Page     { get; set; } = 1;
        public int     PageSize { get; set; } = 20;
    }

    public class ConversationListResult
    {
        public bool                  Success    { get; set; }
        public string                Message    { get; set; } = null!;
        public List<ConversationDTO> Items      { get; set; } = new();
        public int                   Total      { get; set; }
        public int                   Page       { get; set; }
        public int                   PageSize   { get; set; }
        public int                   TotalPages { get; set; }
    }

    /// <summary>Body for POST /hospital/conversations</summary>
    public class StartConversationDTO
    {
        [Required]
        public string DonorId { get; set; } = null!;
    }

    public class ConversationResult
    {
        public bool            Success { get; set; }
        public string          Message { get; set; } = null!;
        public ConversationDTO? Value  { get; set; }
    }

    // ═══════════════════════════════════════════════════
    // MESSAGE DTOs
    // ═══════════════════════════════════════════════════

    public class MessageDTO
    {
        public string   MessageId      { get; set; } = null!;
        public string   ConversationId { get; set; } = null!;
        public string   SenderId       { get; set; } = null!;
        public string   SenderType     { get; set; } = null!; // "Hospital" | "Donor"
        public string?  Text           { get; set; }
        public string   Status         { get; set; } = null!; // Sent | Delivered | Read
        public bool     IsDeleted      { get; set; }
        public DateTime CreatedAt      { get; set; }
        public DateTime? EditedAt      { get; set; }
    }

    /// <summary>Query parameters for GET /hospital/conversations/:id/messages (cursor-based)</summary>
    public class GetMessagesQuery
    {
        public string? Cursor { get; set; } // last MessageId seen — load older than this
        public int     Limit  { get; set; } = 30;
    }

    public class MessageListResult
    {
        public bool          Success    { get; set; }
        public string        Message    { get; set; } = null!;
        public List<MessageDTO> Items   { get; set; } = new();
        public string?       NextCursor { get; set; } // null = no more pages
    }

    /// <summary>Body for POST /hospital/conversations/:id/messages</summary>
    public class SendMessageDTO
    {
        [Required]
        [MaxLength(2000)]
        public string Text { get; set; } = null!;
    }

    public class MessageResult
    {
        public bool       Success { get; set; }
        public string     Message { get; set; } = null!;
        public MessageDTO? Value  { get; set; }
    }

    // ═══════════════════════════════════════════════════
    // UNREAD COUNT
    // ═══════════════════════════════════════════════════

    public class UnreadCountDTO
    {
        public int Total { get; set; }
    }

    public class UnreadCountResult
    {
        public bool           Success { get; set; }
        public string         Message { get; set; } = null!;
        public UnreadCountDTO? Value  { get; set; }
    }

    // ═══════════════════════════════════════════════════
    // SIGNALR PAYLOADS  (server → client events)
    // ═══════════════════════════════════════════════════

    /// <summary>Emitted to all parties when a new message arrives</summary>
    public class NewMessagePayload
    {
        public MessageDTO      MessageDto     { get; set; } = null!;
        public ConversationDTO ConversationDto { get; set; } = null!;
    }

    /// <summary>Emitted when a message is soft-deleted</summary>
    public class MessageDeletedPayload
    {
        public string MessageId      { get; set; } = null!;
        public string ConversationId { get; set; } = null!;
    }

    /// <summary>Emitted to sender's conversation partner when they are typing</summary>
    public class TypingPayload
    {
        public string ConversationId { get; set; } = null!;
        public string SenderId       { get; set; } = null!;
        public string SenderType     { get; set; } = null!;
        public bool   IsTyping       { get; set; }
    }

    /// <summary>Emitted when conversation is marked read</summary>
    public class ReadPayload
    {
        public string ConversationId { get; set; } = null!;
        public string ReadByType     { get; set; } = null!; // "Hospital" | "Donor"
    }

    /// <summary>Emitted when a donor connects / disconnects</summary>
    public class PresencePayload
    {
        public string   DonorId  { get; set; } = null!;
        public bool     Online   { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
