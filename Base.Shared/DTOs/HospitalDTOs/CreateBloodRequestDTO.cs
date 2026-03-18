// Base.Shared/DTOs/HospitalDTOs/CreateBloodRequestDTO.cs

using Base.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Base.Shared.DTOs.HospitalDTOs
{
    public class CreateBloodRequestDTO
    {
        [Required]
        public string BloodTypeId { get; set; }
        // "bt-apos", "bt-aneg", "bt-bpos", "bt-bneg",
        // "bt-abpos", "bt-abneg", "bt-opos", "bt-oneg"

        [Required]
        [Range(1, 100)]
        public int QuantityRequired { get; set; } = 1;

        [Required]
        public UrgencyLevel UrgencyLevel { get; set; }
        // "Routine", "Urgent", "Critical"

        public string? Note { get; set; }

        public DateTime? Deadline { get; set; }
    }
}