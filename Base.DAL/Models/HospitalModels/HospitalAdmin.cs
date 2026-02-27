using Base.DAL.Models.BaseModels;

namespace Base.DAL.Models.HospitalModels
{
    public class HospitalAdmin : BaseEntity
    {
        public string UserId { get; set; }          // FK to ApplicationUser
        public string HospitalId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedById { get; set; }

        // Navigation
        public virtual ApplicationUser User { get; set; }
        public virtual Hospital Hospital { get; set; }
    }
}
