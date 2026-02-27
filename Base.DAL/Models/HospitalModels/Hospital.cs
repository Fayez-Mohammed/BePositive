using Base.DAL.Models.BaseModels;
using Base.DAL.Models.RequestModels;

namespace Base.DAL.Models.HospitalModels
{
    public class Hospital : BaseEntity
    {
        public string Name { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? GovernorateId { get; set; }
        public string? CityId { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Status { get; set; } = "UnderReview"; // "Active","UnderReview","Suspended"
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedById { get; set; }

        // Navigation
        public virtual Governorate? Governorate { get; set; }
        public virtual City? City { get; set; }
        public virtual HospitalAdmin? Admin { get; set; }
        public virtual ICollection<DonationRequest> DonationRequests { get; set; }
        public virtual ICollection<DonationHistory> DonationHistories { get; set; }
    }
}
