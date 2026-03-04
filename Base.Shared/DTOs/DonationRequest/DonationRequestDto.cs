namespace Base.Shared.DTOs;

public class DonationRequestDto
{
    public string HospitalId { get; set; }
    public string BloodTypeId { get; set; }
    public int QuantityRequired { get; set; } = 1;
    public int QuantityFulfilled { get; set; } = 0;
    public string UrgencyLevel { get; set; }    // "Normal", "Urgent", "Critical"
    public string? Note { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime? Deadline { get; set; }
}