// Base.Shared/DTOs/DashboardDTOs/DashboardDTOs.cs

namespace Base.Shared.DTOs.DashboardDTOs
{
    // ── Stats ─────────────────────────────────────────────────────
    public class DashboardStatsDTO
    {
        public StatCardDTO TotalDonors         { get; set; }
        public StatCardDTO AvailableBloodUnits { get; set; }
        public StatCardDTO UrgentRequests      { get; set; }
        public StatCardDTO TransfusionsToday   { get; set; }
    }

    public class StatCardDTO
    {
        public int     Value          { get; set; }
        public double  ChangePercent  { get; set; } // e.g. +12.5 or -2.4
        public string  ChangeLabel    { get; set; } // "vs last month"
    }

    // ── Recent Activity ───────────────────────────────────────────
    public class RecentActivityResult
    {
        public bool                    Success    { get; set; }
        public string                  Message    { get; set; }
        public List<ActivityItemDTO>   Value      { get; set; } = new();
    }

    // ── Activity Log (paginated) ──────────────────────────────────
    public class ActivityLogResult
    {
        public bool                    Success    { get; set; }
        public string                  Message    { get; set; }
        public int                     Total      { get; set; }
        public int                     Page       { get; set; }
        public int                     Limit      { get; set; }
        public int                     TotalPages { get; set; }
        public List<ActivityItemDTO>   Value      { get; set; } = new();
    }

    public class ActivityItemDTO
    {
        public string   Id               { get; set; }
        public string   ActivityType     { get; set; }
        // "BloodDonation" | "NewRequest" | "RequestFulfilled" |
        // "RequestCancelled" | "InventoryAdded" | "InventoryWithdrawn" |
        // "BatchExpired"
        public string   Title            { get; set; }  // e.g. "Blood Donation: O+"
        public string?  BloodTypeId      { get; set; }
        public string?  BloodTypeName    { get; set; }
        public string?  RelatedId        { get; set; }  // RequestId or InventoryTransactionId
        public DateTime OccurredAt       { get; set; }
        public string?  TransactionHash  { get; set; }  // null until blockchain ready
        public bool     IsVerified       { get; set; }  // false until blockchain ready
    }

    // ── Activity Log Query ────────────────────────────────────────
    public class ActivityLogQuery
    {
        public string?   ActivityType { get; set; }  // null = all
        public string?   BloodTypeId  { get; set; }
        public DateOnly? Date         { get; set; }
        public int       Page         { get; set; } = 1;
        public int       Limit        { get; set; } = 10;
    }
}
