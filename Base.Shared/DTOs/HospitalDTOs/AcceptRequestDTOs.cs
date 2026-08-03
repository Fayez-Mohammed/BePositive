using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Base.Shared.DTOs.DonorDTOs
{
    public class AcceptRequestDTO
    {
        [Required(ErrorMessage = "RequestId is required.")]
        [JsonPropertyName("requestid")]
        public string RequestId { get; set; } = string.Empty;
    }
    public class AcceptRequestResultDTO
    {
        [JsonPropertyName("hospitalname")]
        public string HospitalName { get; set; } = string.Empty;

        [JsonPropertyName("hospitallatitude")]
        public double? HospitalLatitude { get; set; }

        [JsonPropertyName("hospitallongitude")]
        public double? HospitalLongitude { get; set; }
    }
    public class NearbyRequestQueryDTO
    {
        public double MaxDistanceKm { get; set; } = 15.0; // المسافة الافتراضية للبحث 15 كم لو متبعتش حاجة
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DonorNearbyRequestDTO
    {
        public string RequestId { get; set; } = null!;
        public string HospitalName { get; set; } = null!;
        public string BloodTypeName { get; set; } = null!;
        public string UrgencyLevel { get; set; }
        public string? Note { get; set; }
        public double DistanceKm { get; set; } // المسافة المحسوبة بالـ Haversine
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime DateOfCreation { get; set; }
    }

    public class DonorNearbyRequestListResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public List<DonorNearbyRequestDTO> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}