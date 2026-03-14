using Base.Shared.Enums;

namespace Base.Shared.DTOs.HospitalDTOs
{
    // Base.Shared/DTOs/Auth/Hospital/HospitalRegisterResponse.cs

    public class HospitalRegisterResponse
    {
        public string HospitalId { get; set; }
        public string AdminUserId { get; set; }
        public string HospitalName { get; set; }
        public string Email { get; set; }
        public HospitalStatus Status { get; set; }          // Always "UnderReview" on register
        public string Message { get; set; }
    }
}
