using Base.DAL.Models.BaseModels;

namespace Base.DAL.Models.BloodModels
{
    public class BloodTypeCompatibility : BaseEntity
    {
        public string DonorBloodTypeId { get; set; }
        public string RecipientBloodTypeId { get; set; }

        // Navigation
        public virtual BloodType DonorBloodType { get; set; }
        public virtual BloodType RecipientBloodType { get; set; }
    }
}
