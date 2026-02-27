using Base.DAL.Models.BaseModels;
using Base.DAL.Models.BloodModels;
using Base.DAL.Models.HospitalModels;
using Base.DAL.Models.RequestModels;

namespace Base.DAL.Models.DonorModels
{
    public class Donor : BaseEntity
    {
        public string UserId { get; set; }          // FK to ApplicationUser
        public string? NationalId { get; set; }
        public string BloodTypeId { get; set; }
        public string? CityId { get; set; }
        public string? Gender { get; set; }         // "Male", "Female"
        public DateOnly? BirthDate { get; set; }
        public DateOnly? LastDonationDate { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsAvailableForDonation { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedById { get; set; }

        // Navigation
        public virtual ApplicationUser User { get; set; }
        public virtual BloodType BloodType { get; set; }
        public virtual City? City { get; set; }
        public virtual ICollection<RequestResponse> RequestResponses { get; set; }
        public virtual ICollection<DonationHistory> DonationHistories { get; set; }
        public virtual ICollection<DonorNotificationLog> NotificationLogs { get; set; }
    }
}
