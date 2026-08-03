using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Base.Shared.DTOs.DonorDTOs
{
    public class CompleteProfileDTO
    {
        [Required(ErrorMessage = "National ID is required.")]
        [JsonPropertyName("nationalid")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Blood Type is required.")]
        [JsonPropertyName("bloodtypeid")]
        public string BloodTypeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [JsonPropertyName("cityid")]
        public string CityId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty; // "Male" or "Female"

        [Required(ErrorMessage = "Birth date is required.")]
        [JsonPropertyName("birthdate")]
        public DateOnly BirthDate { get; set; }

        [JsonPropertyName("lastdonationdate")]
        public DateOnly? LastDonationDate { get; set; }

        [Required(ErrorMessage = "Latitude is required.")]
        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        [Required(ErrorMessage = "FCM Token is required.")]
        [JsonPropertyName("fcmtoken")]
        public string FcmToken { get; set; } = string.Empty;
    }
    public class UpdateFcmDTO
    {
        [Required(ErrorMessage = "FCM Token is required.")]
        [JsonPropertyName("fcmtoken")]
        public string FcmToken { get; set; } = string.Empty;
    }
}