// Base.Services/Implementations/BloodRequestService.cs

using Base.DAL.Contexts;
using Base.DAL.Models.RequestModels;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly AppDbContext _context;

        public BloodRequestService(AppDbContext context)
        {
            _context = context;
        }

        // ── Create ────────────────────────────────────────────────────
        public async Task<BloodRequestResponseDTO> CreateRequestAsync(
            string hospitalAdminUserId,
            CreateBloodRequestDTO dto)
        {
            // 1. Get hospital from admin user
            var hospitalAdmin = await _context.HospitalAdmins
                .Include(ha => ha.Hospital)
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId &&
                    !ha.IsDeleted);

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException(
                    "No active hospital admin record found.");

            var hospital = hospitalAdmin.Hospital;

            if (hospital == null || hospital.IsDeleted)
                throw new UnauthorizedAccessException(
                    "Associated hospital not found.");

            if (hospital.Status != HospitalStatus.Active)
                throw new InvalidOperationException(
                    "Your hospital must be active to create blood requests.");

            // 2. Validate blood type exists
            var bloodType = await _context.BloodTypes
                .FirstOrDefaultAsync(b => b.Id == dto.BloodTypeId);

            if (bloodType == null)
                throw new ArgumentException("Invalid blood type selected.");

            // 3. Create the donation request
            var request = new DonationRequest
            {
                HospitalId = hospital.Id,
                BloodTypeId = dto.BloodTypeId,
                QuantityRequired = dto.QuantityRequired,
                QuantityFulfilled = 0,
                UrgencyLevel = dto.UrgencyLevel,
                Note = dto.Note,
                Deadline = dto.Deadline,
                Status = RequestStatus.Open,
                Latitude = hospital.Latitude,
                Longitude = hospital.Longitude,
                IsDeleted = false
            };

            _context.DonationRequests.Add(request);
            await _context.SaveChangesAsync();

            // 4. Create in-app notification for the hospital admin
            var notification = new Notification
            {
                UserId = hospitalAdminUserId,
                Title = "Blood Request Created",
                Body = $"Your request for {bloodType.TypeName} blood has been submitted successfully.",
                IsRead = false,
                RelatedRequestId = request.Id
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // 5. Return response
            return new BloodRequestResponseDTO
            {
                Id = request.Id,
                HospitalId = hospital.Id,
                HospitalName = hospital.Name,
                BloodTypeId = bloodType.Id,
                BloodTypeName = bloodType.TypeName,
                QuantityRequired = request.QuantityRequired,
                QuantityFulfilled = 0,
                UrgencyLevel = request.UrgencyLevel,
                Status = request.Status,
                Note = request.Note,
                Deadline = request.Deadline,
                CreatedAt = request.DateOfCreattion
            };
        }

        // ── Get All ───────────────────────────────────────────────────
        public async Task<BloodRequestListResult> GetAllRequestsAsync(
            string hospitalAdminUserId,
            GetBloodRequestsQuery query)
        {
            var hospitalAdmin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No hospital admin record found.");

            var q = _context.DonationRequests
                .AsNoTracking()
                .Include(r => r.BloodType)
                .Where(r => r.HospitalId == hospitalAdmin.HospitalId)
                .AsQueryable();

            // ── Filters ───────────────────────────────────────────────
            if (query.Status.HasValue)
                q = q.Where(r => r.Status == query.Status.Value);

            if (query.UrgencyLevel.HasValue)
                q = q.Where(r => r.UrgencyLevel == query.UrgencyLevel.Value);

            if (!string.IsNullOrWhiteSpace(query.BloodTypeId))
                q = q.Where(r => r.BloodTypeId == query.BloodTypeId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(r =>
                    r.Id.ToLower().Contains(search) ||
                    (r.Note != null && r.Note.ToLower().Contains(search)));
            }

            var total = await q.CountAsync();

            // ── Main query ────────────────────────────────────────────
            var requests = await q
                .OrderByDescending(r => r.DateOfCreattion)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(r => new BloodRequestSummaryDTO
                {
                    Id = r.Id,
                    HospitalId = r.HospitalId,
                    BloodTypeId = r.BloodTypeId,
                    BloodTypeName = r.BloodType.TypeName,
                    QuantityRequired = r.QuantityRequired,
                    QuantityFulfilled = r.QuantityFulfilled,
                    ProgressPercent = r.QuantityRequired == 0 ? 0
                                        : Math.Round(
                                            (double)r.QuantityFulfilled / r.QuantityRequired * 100, 1),
                    UrgencyLevel = r.UrgencyLevel,
                    Status = r.Status,
                    Note = r.Note,
                    Deadline = r.Deadline,
                    CreatedAt = r.DateOfCreattion
                })
                .ToListAsync();

            return new BloodRequestListResult
            {
                Success = true,
                Message = "Requests retrieved successfully.",
                Total = total,
                Page = query.Page,
                Limit = query.Limit,
                TotalPages = (int)Math.Ceiling((double)total / query.Limit),
                Value = requests
            };
        }

        // ── Get By Id ─────────────────────────────────────────────────
        public async Task<BloodRequestDetailResult> GetRequestByIdAsync(
            string hospitalAdminUserId,
            string requestId)
        {
            var hospitalAdmin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No hospital admin record found.");

            var raw = await _context.DonationRequests
                .AsNoTracking()
                .Include(r => r.BloodType)
                .Where(r =>
                    r.Id == requestId &&
                    r.HospitalId == hospitalAdmin.HospitalId)
                .Select(r => new
                {
                    r.Id,
                    r.HospitalId,
                    r.BloodTypeId,
                    BloodTypeName = r.BloodType.TypeName,
                    r.QuantityRequired,
                    r.QuantityFulfilled,
                    r.UrgencyLevel,
                    r.Status,
                    r.Note,
                    r.Deadline,
                    r.Latitude,
                    r.Longitude,
                    r.DateOfCreattion
                })
                .FirstOrDefaultAsync();

            if (raw == null)
                return new BloodRequestDetailResult
                {
                    Success = false,
                    Message = "Request not found."
                };

            // ── Response counts ───────────────────────────────────────
            var responseCounts = await _context.RequestResponses
                .AsNoTracking()
                .Where(rr => rr.RequestId == requestId)
                .GroupBy(rr => rr.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            int totalResponses = responseCounts.Sum(x => x.Count);
            int accepted = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.Accepted)?.Count ?? 0;
            int arrived = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.Arrived)?.Count ?? 0;
            int donated = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.Donated)?.Count ?? 0;
            int noShow = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.NoShow)?.Count ?? 0;

            return new BloodRequestDetailResult
            {
                Success = true,
                Message = "Request retrieved successfully.",
                Value = new BloodRequestDetailDTO
                {
                    Id = raw.Id,
                    HospitalId = raw.HospitalId,
                    BloodTypeId = raw.BloodTypeId,
                    BloodTypeName = raw.BloodTypeName,
                    QuantityRequired = raw.QuantityRequired,
                    QuantityFulfilled = raw.QuantityFulfilled,
                    ProgressPercent = raw.QuantityRequired == 0 ? 0
                                        : Math.Round(
                                            (double)raw.QuantityFulfilled / raw.QuantityRequired * 100, 1),
                    UrgencyLevel = raw.UrgencyLevel,
                    Status = raw.Status,
                    Note = raw.Note,
                    Deadline = raw.Deadline,
                    Latitude = raw.Latitude,
                    Longitude = raw.Longitude,
                    CreatedAt = raw.DateOfCreattion,
                    Responses = totalResponses,
                    Accepted = accepted,
                    Arrived = arrived,
                    Donated = donated,
                    NoShow = noShow
                }
            };
        }

        // ── Update ────────────────────────────────────────────────────
        public async Task<BloodRequestResponseDTO> UpdateRequestAsync(
            string hospitalAdminUserId,
            string requestId,
            UpdateBloodRequestDTO dto)
        {
            var hospitalAdmin = await _context.HospitalAdmins
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No hospital admin record found.");

            var request = await _context.DonationRequests
                .Include(r => r.BloodType)
                .FirstOrDefaultAsync(r =>
                    r.Id == requestId &&
                    r.HospitalId == hospitalAdmin.HospitalId);

            if (request == null)
                throw new KeyNotFoundException("Request not found.");

            if (request.Status != RequestStatus.Open)
                throw new InvalidOperationException(
                    "Only open requests can be updated.");

            // Apply updates — only update fields that are provided
            if (dto.QuantityRequired.HasValue)
                request.QuantityRequired = dto.QuantityRequired.Value;

            if (dto.UrgencyLevel.HasValue)
                request.UrgencyLevel = dto.UrgencyLevel.Value;

            if (dto.Note is not null)
                request.Note = dto.Note;

            if (dto.Deadline.HasValue)
                request.Deadline = dto.Deadline;

            if (dto.Status.HasValue && dto.Status == RequestStatus.Cancelled)
                request.Status = RequestStatus.Cancelled;

            await _context.SaveChangesAsync();

            return new BloodRequestResponseDTO
            {
                Id = request.Id,
                HospitalId = request.HospitalId,
                HospitalName = hospitalAdmin.Hospital?.Name ?? "",
                BloodTypeId = request.BloodTypeId,
                BloodTypeName = request.BloodType?.TypeName ?? "",
                QuantityRequired = request.QuantityRequired,
                QuantityFulfilled = request.QuantityFulfilled,
                UrgencyLevel = request.UrgencyLevel,
                Status = request.Status,
                Note = request.Note,
                Deadline = request.Deadline,
                CreatedAt = request.DateOfCreattion
            };
        }

        // ── Cancel ────────────────────────────────────────────────────
        public async Task<bool> CancelRequestAsync(
            string hospitalAdminUserId,
            string requestId)
        {
            var hospitalAdmin = await _context.HospitalAdmins
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No hospital admin record found.");

            var request = await _context.DonationRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == requestId &&
                    r.HospitalId == hospitalAdmin.HospitalId);

            if (request == null)
                throw new KeyNotFoundException("Request not found.");

            if (request.Status != RequestStatus.Open)
                throw new InvalidOperationException(
                    "Only open requests can be cancelled.");

            request.Status = RequestStatus.Cancelled;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}