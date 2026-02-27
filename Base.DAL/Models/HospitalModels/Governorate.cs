using Base.DAL.Models.BaseModels;
using Base.DAL.Models.DonorModels;

namespace Base.DAL.Models.HospitalModels
{
    public class Governorate : BaseEntity
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }

        // Navigation
        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<Hospital> Hospitals { get; set; }
    }
}
