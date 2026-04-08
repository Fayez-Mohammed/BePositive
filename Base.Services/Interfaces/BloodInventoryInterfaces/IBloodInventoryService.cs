// Base.Services/Interfaces/HospitalInterfaces/IBloodInventoryService.cs

using Base.Shared.DTOs.InventoryDTOs;

namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IBloodInventoryService
    {
        Task<InventoryOverviewResult>    GetInventoryAsync(string hospitalAdminUserId);
        Task<InventoryDetailResult>      GetInventoryByBloodTypeAsync(string hospitalAdminUserId, string bloodTypeId);
        Task<AddBatchResult>             AddBatchAsync(string hospitalAdminUserId, AddBatchDTO dto);
        Task<WithdrawResult>             WithdrawAsync(string hospitalAdminUserId, WithdrawDTO dto);
        Task<ExpiringSoonResult>         GetExpiringSoonAsync(string hospitalAdminUserId, int days = 7);
        Task<TransactionListResult>      GetTransactionsAsync(string hospitalAdminUserId, int page = 1, int limit = 10);
        Task<CompatibleInventoryResult>  GetCompatibleInventoryAsync(string hospitalAdminUserId, string bloodTypeId);
    }
}
