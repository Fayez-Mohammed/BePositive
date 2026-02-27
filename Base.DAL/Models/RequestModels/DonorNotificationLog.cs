using Base.DAL.Models.BaseModels;
using Base.DAL.Models.DonorModels;

namespace Base.DAL.Models.RequestModels
{
    public class DonorNotificationLog : BaseEntity
    {
        public string DonorId { get; set; }
        public string RequestId { get; set; }
        public string? Channel { get; set; }    // "Push", "SMS"
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsDelivered { get; set; } = false;

        // Navigation
        public virtual Donor Donor { get; set; }
        public virtual DonationRequest Request { get; set; }
    }
}
