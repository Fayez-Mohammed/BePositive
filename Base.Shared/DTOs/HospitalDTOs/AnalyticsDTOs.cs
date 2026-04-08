// Base.Shared/DTOs/AnalyticsDTOs/AnalyticsDTOs.cs

namespace Base.Shared.DTOs.AnalyticsDTOs
{
    // ── Period enum ───────────────────────────────────────────────
    public enum AnalyticsPeriod
    {
        Last7Days   = 1,
        Last30Days  = 2,
        Last3Months = 3,
        LastYear    = 4
    }

    // ── Summary (3 stat cards) ────────────────────────────────────
    public class AnalyticsSummaryDTO
    {
        public StatDTO TotalDonations    { get; set; }
        public StatDTO ResponseRate      { get; set; }
        public StatDTO AvgFulfillmentHrs { get; set; }
        public string  PeriodLabel       { get; set; }
        // e.g. "Last 7 Days", "Last 30 Days", "Last 3 Months", "Last Year"
    }

    public class StatDTO
    {
        public double Value         { get; set; }
        public double ChangePercent { get; set; } // vs previous period
    }

    // ── Trends (line chart) ───────────────────────────────────────
    public class TrendsDTO
    {
        public List<string> Labels    { get; set; } = new();
        // Last7Days   → ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"]
        // Last30Days  → ["Apr 1","Apr 2"...] daily
        // Last3Months → ["Week 1","Week 2"...] weekly
        // LastYear    → ["Jan","Feb"..."Dec"] monthly

        public List<int>    Donations { get; set; } = new();
        public List<int>    Requests  { get; set; } = new();
    }

    // ── Blood Type Distribution (donut chart) ─────────────────────
    public class BloodTypeDistributionResult
    {
        public bool                          Success { get; set; }
        public string                        Message { get; set; }
        public List<BloodTypeDistributionDTO> Value  { get; set; } = new();
    }

    public class BloodTypeDistributionDTO
    {
        public string BloodTypeId   { get; set; }
        public string BloodTypeName { get; set; }
        public int    Count         { get; set; }
        public double Percentage    { get; set; }
    }
}
