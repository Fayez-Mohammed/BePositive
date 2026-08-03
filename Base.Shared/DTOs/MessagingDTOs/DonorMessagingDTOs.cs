using System.ComponentModel.DataAnnotations;
// Base.Shared/DTOs/MessagingDTOs/DonorConversationDTO.cs
namespace Base.Shared.DTOs.MessagingDTOs
{
    public class DonorConversationDTO
    {
        public string ConversationId { get; set; } = null!;
        public string HospitalId { get; set; } = null!;
        public string HospitalName { get; set; } = null!;
        public string? HospitalCity { get; set; }
        public bool Online { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class DonorConversationListResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public List<DonorConversationDTO> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class DonorMessageDTO
    {
        public string MessageId { get; set; } = null!;
        public string ConversationId { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public string SenderType { get; set; } = null!; // "Hospital" or "Donor"
        public string? Text { get; set; }
        public string Status { get; set; } = null!; // "Sent", "Read"
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DonorMessageListResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public List<DonorMessageDTO> Items { get; set; } = new();
        public string? NextCursor { get; set; }
    }


}