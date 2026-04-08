// Main inventory — current levels per hospital per blood type
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.BloodModels;
using Base.DAL.Models.HospitalModels;
using Base.Shared.Enums;
// Batch tracking — for expiry
namespace Base.DAL.Models.InventoryModels
{
    public class BloodInventoryBatch : BaseEntity
    {
        public string HospitalId { get; set; }
        public string BloodTypeId { get; set; }
        public string BloodInventoryId { get; set; }  // ← add this, not nullable

        public int Units { get; set; }
        public int RemainingUnits { get; set; }
        public DateOnly CollectionDate { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public BatchStatus Status { get; set; } // Active, Expired, Depleted

        public virtual Hospital Hospital { get; set; }
        public virtual BloodType BloodType { get; set; }
        public virtual BloodInventory BloodInventory { get; set; }

    }
}