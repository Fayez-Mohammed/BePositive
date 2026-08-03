// Base.Shared/DTOs/HospitalDTOs/EligibleDonorsCountDTOs.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Base.Shared.DTOs.HospitalDTOs
{
    public class GetEligibleDonorsCountDTO
    {
        [Required]
        [JsonPropertyName("bloodTypeId")]
        public string BloodTypeId { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 100.0, ErrorMessage = "Distance must be between 0.1 and 100 KM.")]
        [JsonPropertyName("maxDistanceKm")]
        public double MaxDistanceKm { get; set; } = 10.0;
    }

    public class EligibleDonorsCountResponseDTO
    {
        // تم تغيير الـ Mapping هنا ليكون حروف صغيرة ملتصقة تماماً
        // لتطابق التعديل الجبري (prop.Name.ToLower) الذي يحدث داخل الـ Middleware
        [JsonPropertyName("bloodtypeid")]
        public string BloodTypeId { get; set; } = string.Empty;

        [JsonPropertyName("bloodtypename")]
        public string BloodTypeName { get; set; } = string.Empty;

        [JsonPropertyName("searcheddistancekm")]
        public double SearchedDistanceKm { get; set; }

        [JsonPropertyName("totaleligibledonorscount")]
        public int TotalEligibleDonorsCount { get; set; }
    }
}