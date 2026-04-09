using Base.DAL.Models.BaseModels;
using Base.DAL.Models.DonorModels;
using Base.DAL.Models.HospitalModels;

namespace Base.DAL.Models.RequestModels
{
    public class DonationHistory : BaseEntity
    {
        public string DonorId { get; set; }
        public string? HospitalId { get; set; }
        public string? RequestId { get; set; }      
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
        public int AmountML { get; set; } = 450;
        public string? Report { get; set; }

        // Navigation
        public virtual Donor Donor { get; set; }
        public virtual Hospital? Hospital { get; set; }
        public virtual DonationRequest? Request { get; set; }
    }
}
