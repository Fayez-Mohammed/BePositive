using Base.DAL.Models.BaseModels;
using Base.DAL.Models.DonorModels;
using Base.Shared.Enums;

namespace Base.DAL.Models.RequestModels
{
    public class RequestResponse : BaseEntity
    {
        public string RequestId { get; set; }
        public string DonorId { get; set; }
        public ResponseStatus Status { get; set; } = ResponseStatus.Pending;
        // "Pending","Accepted","Arrived","Donated","NoShow","Rejected"
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
        public decimal? DonorDistanceKm { get; set; }

        // Navigation
        public virtual DonationRequest Request { get; set; }
        public virtual Donor Donor { get; set; }
    }
}
