using Base.DAL.Models.BaseModels;

namespace Base.DAL.Models.RequestModels
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; }          // FK to ApplicationUser
        public string? Title { get; set; }
        public string? Body { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RelatedRequestId { get; set; }

        // Navigation
        public virtual ApplicationUser User { get; set; }
        public virtual DonationRequest? RelatedRequest { get; set; }
    }
}
