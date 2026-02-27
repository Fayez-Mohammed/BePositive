using Base.DAL.Models.BaseModels;
using Base.DAL.Models.DonorModels;
using Base.DAL.Models.RequestModels;

namespace Base.DAL.Models.BloodModels
{
    public class BloodType : BaseEntity
    {
        public string TypeName { get; set; }    // "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"

        // Navigation
        public virtual ICollection<BloodTypeCompatibility> CanDonateTo { get; set; }
        public virtual ICollection<BloodTypeCompatibility> CanReceiveFrom { get; set; }
        public virtual ICollection<Donor> Donors { get; set; }
        public virtual ICollection<DonationRequest> DonationRequests { get; set; }
    }
}
