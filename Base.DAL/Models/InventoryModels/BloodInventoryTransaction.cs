// Main inventory — current levels per hospital per blood type
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.BloodModels;
using Base.DAL.Models.HospitalModels;
using Base.Shared.Enums;
// Audit log — every change tracked
namespace Base.DAL.Models.InventoryModels
{
    public class BloodInventoryTransaction : BaseEntity
    {
        public string HospitalId { get; set; }
        public string BloodTypeId { get; set; }
        public string BloodInventoryId { get; set; }  // ← add this, not nullable

        public int ChangeAmount { get; set; } // positive = add, negative = withdraw
        public TransactionReason Reason { get; set; } // ManualAdd, ManualWithdraw, RequestFulfillment, Expired
        public string? RequestId { get; set; } // if linked to a donation request
        public string ChangedById { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }

        public virtual Hospital Hospital { get; set; }
        public virtual BloodType BloodType { get; set; }
        public virtual BloodInventory BloodInventory { get; set; }

    }
}