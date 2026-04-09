using System;
using System.ComponentModel.DataAnnotations;

namespace Base.DAL.Models.RequestModels.DTOs
{
    public class DonationHistoryDto
    {
        public string Id { get; set; }
        public string DonorId { get; set; }
        public string? HospitalId { get; set; }
        public string? RequestId { get; set; }
        public DateTime DonationDate { get; set; }
        public int AmountML { get; set; }
        public string? Report { get; set; }
    }

    public class CreateDonationHistoryDto
    {
        [Required]
        public string DonorId { get; set; }
        public string? HospitalId { get; set; }
        public string? RequestId { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
        public int AmountML { get; set; } = 450;
        public string? Report { get; set; }
    }

    public class UpdateDonationHistoryDto
    {
        [Required]
        public string Id { get; set; }
        public string? HospitalId { get; set; }
        public string? RequestId { get; set; }
        public DateTime DonationDate { get; set; }
        public int AmountML { get; set; }
        public string? Report { get; set; }
    }
}