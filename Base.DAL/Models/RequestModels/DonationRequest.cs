using Base.DAL.Models.BaseModels;
using Base.DAL.Models.BloodModels;
using Base.DAL.Models.HospitalModels;

namespace Base.DAL.Models.RequestModels
{
    public class DonationRequest : BaseEntity
    {
        public string HospitalId { get; set; }
        public string BloodTypeId { get; set; }
        public int QuantityRequired { get; set; } = 1;
        public int QuantityFulfilled { get; set; } = 0;
        public string UrgencyLevel { get; set; }    // "Normal", "Urgent", "Critical"
        public string? Note { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Status { get; set; } = "Open"; // "Open","Fulfilled","Cancelled","Expired"
        public DateTime? Deadline { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedById { get; set; }

        // Navigation
        public virtual Hospital Hospital { get; set; }
        public virtual BloodType BloodType { get; set; }
        public virtual ICollection<RequestResponse> Responses { get; set; }
        public virtual ICollection<DonorNotificationLog> NotificationLogs { get; set; }
        public virtual ICollection<DonationHistory> DonationHistories { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
