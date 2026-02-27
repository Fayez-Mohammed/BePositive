using Base.DAL.Models.BaseModels;
using Base.DAL.Models.DonorModels;

namespace Base.DAL.Models.HospitalModels
{
    public class City : BaseEntity
    {
        public string GovernorateId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }

        // Navigation
        public virtual Governorate Governorate { get; set; }
        public virtual ICollection<Hospital> Hospitals { get; set; }
        public virtual ICollection<Donor> Donors { get; set; }
    }
}
