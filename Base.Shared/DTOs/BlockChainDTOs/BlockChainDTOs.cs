// Base.Shared/DTOs/BlockchainRequestDTO.cs
using System.Text.Json.Serialization;

namespace Base.Shared.DTOs
{
    public class BlockchainRequestDTO
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("hospitalId")]
        public string HospitalId { get; set; } = string.Empty;

        [JsonPropertyName("bloodTypeId")]
        public string BloodTypeId { get; set; } = string.Empty;

        [JsonPropertyName("quantityRequired")]
        public int QuantityRequired { get; set; }

        [JsonPropertyName("urgencyLevel")]
        public int UrgencyLevel { get; set; }

        [JsonPropertyName("deadline")]
        public string Deadline { get; set; } = string.Empty; // ISO string format
    }

    public class BlockchainResponseDTO
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("transactionHash")]
        public string TransactionHash { get; set; } = string.Empty;

        [JsonPropertyName("blockNumber")]
        public int BlockNumber { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;
    }
}