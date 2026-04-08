// Main inventory — current levels per hospital per blood type
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.BloodModels;
using Base.DAL.Models.HospitalModels;
namespace Base.DAL.Models.InventoryModels
{
    public class BloodInventory : BaseEntity
    {
        public string HospitalId { get; set; }
        public string BloodTypeId { get; set; }
        public int TotalUnits { get; set; } // sum of all active batches
        public DateTime LastUpdated { get; set; }

        public virtual Hospital Hospital { get; set; }
        public virtual BloodType BloodType { get; set; }
        public virtual ICollection<BloodInventoryBatch> Batches { get; set; }
        public virtual ICollection<BloodInventoryTransaction> Transactions { get; set; }
    }
}