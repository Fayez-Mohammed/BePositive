// Base.Shared/DTOs/BlockchainUpdateDTOs.cs
using System.Text.Json.Serialization;

namespace Base.Shared.DTOs
{
    public class BlockchainStatusUpdateDTO
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    public class BlockchainFulfillmentOnlyDTO //BlockchainFulfilledUpdateDTO
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}

  
