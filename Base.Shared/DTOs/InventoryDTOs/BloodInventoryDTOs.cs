// Base.Shared/DTOs/InventoryDTOs/BloodInventoryDTOs.cs

using Base.Shared.Enums;

namespace Base.Shared.DTOs.InventoryDTOs
{
    // ── Shared sub-objects ────────────────────────────────────────

    public class BatchSummaryDTO
    {
        public string     Id             { get; set; }
        public int        Units          { get; set; }
        public int        RemainingUnits { get; set; }
        public DateOnly   CollectionDate { get; set; }
        public DateOnly   ExpiryDate     { get; set; }
        public int        DaysUntilExpiry { get; set; }
        public BatchStatus Status        { get; set; }
    }

    // ── GET /api/hospital/inventory ───────────────────────────────

    public class InventoryOverviewResult
    {
        public bool                      Success  { get; set; }
        public string                    Message  { get; set; }
        public List<InventoryItemDTO>    Value    { get; set; } = new();
    }

    public class InventoryItemDTO
    {
        public string           InventoryId      { get; set; }
        public string           BloodTypeId      { get; set; }
        public string           BloodTypeName    { get; set; }
        public int              TotalUnits       { get; set; }
        public int              ExpiringIn7Days  { get; set; }
        public int              BatchCount       { get; set; }
        public DateOnly?        NearestExpiry    { get; set; }
    }

    // ── GET /api/hospital/inventory/{bloodTypeId} ─────────────────

    public class InventoryDetailResult
    {
        public bool                Success { get; set; }
        public string              Message { get; set; }
        public InventoryDetailDTO? Value   { get; set; }
    }

    public class InventoryDetailDTO
    {
        public string              InventoryId   { get; set; }
        public string              BloodTypeId   { get; set; }
        public string              BloodTypeName { get; set; }
        public int                 TotalUnits    { get; set; }
        public List<BatchSummaryDTO> Batches     { get; set; } = new();
    }

    // ── POST /api/hospital/inventory/batches/add ──────────────────

    public class AddBatchDTO
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string  BloodTypeId    { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(1, 1000)]
        public int     Units          { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public DateOnly CollectionDate { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public DateOnly ExpiryDate    { get; set; }

        public string? Notes          { get; set; }
    }

    public class AddBatchResult
    {
        public bool           Success { get; set; }
        public string         Message { get; set; }
        public BatchSummaryDTO? Value { get; set; }
    }

    // ── POST /api/hospital/inventory/withdraw ─────────────────────

    public class WithdrawDTO
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string BloodTypeId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(1, 1000)]
        public int    Units       { get; set; }

        public string? RequestId  { get; set; }
        public string? Notes      { get; set; }
    }

    public class WithdrawResult
    {
        public bool   Success          { get; set; }
        public string Message          { get; set; }
        public string BloodTypeId      { get; set; }
        public string BloodTypeName    { get; set; }
        public int    UnitsWithdrawn   { get; set; }
        public int    RemainingUnits   { get; set; }
    }

    // ── GET /api/hospital/inventory/expiring-soon ─────────────────

    public class ExpiringSoonResult
    {
        public bool                    Success { get; set; }
        public string                  Message { get; set; }
        public List<ExpiringSoonDTO>   Value   { get; set; } = new();
    }

    public class ExpiringSoonDTO
    {
        public string    BatchId        { get; set; }
        public string    BloodTypeId    { get; set; }
        public string    BloodTypeName  { get; set; }
        public int       RemainingUnits { get; set; }
        public DateOnly  ExpiryDate     { get; set; }
        public int       DaysUntilExpiry { get; set; }
    }

    // ── GET /api/hospital/inventory/transactions ──────────────────

    public class TransactionListResult
    {
        public bool                       Success    { get; set; }
        public string                     Message    { get; set; }
        public int                        Total      { get; set; }
        public int                        Page       { get; set; }
        public int                        Limit      { get; set; }
        public int                        TotalPages { get; set; }
        public List<TransactionDTO>       Value      { get; set; } = new();
    }

    public class TransactionDTO
    {
        public string            Id            { get; set; }
        public string            BloodTypeId   { get; set; }
        public string            BloodTypeName { get; set; }
        public int               ChangeAmount  { get; set; }
        public TransactionReason Reason        { get; set; }
        public string?           RequestId     { get; set; }
        public string?           Notes         { get; set; }
        public DateTime          ChangedAt     { get; set; }
    }

    // ── GET /api/hospital/inventory/compatible/{bloodTypeId} ──────

    public class CompatibleInventoryResult
    {
        public bool                         Success       { get; set; }
        public string                       Message       { get; set; }
        public string                       RequestedType { get; set; }
        public int                          ExactUnits    { get; set; }
        public List<CompatibleInventoryDTO> Compatible    { get; set; } = new();
        public int                          TotalAvailable { get; set; }
    }

    public class CompatibleInventoryDTO
    {
        public string BloodTypeId   { get; set; }
        public string BloodTypeName { get; set; }
        public int    AvailableUnits { get; set; }
        public bool   IsExactMatch  { get; set; }
    }
}
