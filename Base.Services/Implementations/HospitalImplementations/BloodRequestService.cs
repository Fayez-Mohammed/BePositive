// Base.Services/Implementations/BloodRequestService.cs

using Base.DAL.Contexts;
using Base.DAL.Models.RequestModels;
using Base.Services.HangfireJobs;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Base.Shared.Enums;
using Base.Services.HangFireJobs;
using Microsoft.EntityFrameworkCore;
using Hangfire;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly AppDbContext _context;

        public BloodRequestService(AppDbContext context)
        {
            _context = context;
        }

        //// ── Create ────────────────────────────────────────────────────
        //public async Task<BloodRequestResponseDTO> CreateRequestAsync(
        //    string hospitalAdminUserId,
        //    CreateBloodRequestDTO dto)
        //{
        //    // 1. Get hospital admin — scalars only to avoid enum cast issues
        //    var hospitalAdmin = await _context.HospitalAdmins
        //        .AsNoTracking()
        //        .Where(ha => ha.UserId == hospitalAdminUserId && !ha.IsDeleted)
        //        .Select(ha => new { ha.HospitalId })
        //        .FirstOrDefaultAsync();

        //    if (hospitalAdmin == null)
        //        throw new UnauthorizedAccessException(
        //            "No active hospital admin record found.");

        //    // 2. Get hospital — cast Status to int to avoid LazyLoadingProxy error
        //    var hospital = await _context.Hospitals
        //        .AsNoTracking()
        //        .Where(h => h.Id == hospitalAdmin.HospitalId && !h.IsDeleted)
        //        .Select(h => new
        //        {
        //            h.Id,
        //            h.Name,
        //            h.Latitude,
        //            h.Longitude,
        //            StatusInt = (int)h.Status
        //        })
        //        .FirstOrDefaultAsync();

        //    if (hospital == null)
        //        throw new UnauthorizedAccessException(
        //            "Associated hospital not found.");

        //    // HospitalStatus.Active = 2
        //    if (hospital.StatusInt != 2)
        //        throw new InvalidOperationException(
        //            "Your hospital must be active to create blood requests.");

        //    // 3. Validate blood type
        //    var bloodType = await _context.BloodTypes
        //        .AsNoTracking()
        //        .Where(b => b.Id == dto.BloodTypeId)
        //        .Select(b => new { b.Id, b.TypeName })
        //        .FirstOrDefaultAsync();

        //    if (bloodType == null)
        //        throw new ArgumentException("Invalid blood type selected.");

        //    // 4. Create the donation request
        //    var request = new DonationRequest
        //    {
        //        HospitalId        = hospital.Id,
        //        BloodTypeId       = dto.BloodTypeId,
        //        QuantityRequired  = dto.QuantityRequired,
        //        QuantityFulfilled = 0,
        //        UrgencyLevel      = dto.UrgencyLevel,
        //        Note              = dto.Note,
        //        Deadline          = dto.Deadline,
        //        Status            = RequestStatus.Open,
        //        Latitude          = hospital.Latitude,
        //        Longitude         = hospital.Longitude,
        //        IsDeleted         = false
        //    };

        //    _context.DonationRequests.Add(request);
        //    await _context.SaveChangesAsync();

        //    // 5. Create in-app notification for the hospital admin
        //    var notification = new Notification
        //    {
        //        UserId           = hospitalAdminUserId,
        //        Title            = "Blood Request Created",
        //        Body             = $"Your request for {bloodType.TypeName} blood has been submitted successfully.",
        //        IsRead           = false,
        //        RelatedRequestId = request.Id
        //    };

        //    _context.Notifications.Add(notification);
        //    await _context.SaveChangesAsync();

        //    // 6. Enqueue Hangfire job to find eligible nearby donors and notify them
        //    BackgroundJob.Enqueue<FindAndNotifyDonorsJob>(
        //        job => job.ExecuteAsync(request.Id));

        //    // 7. Return response
        //    return new BloodRequestResponseDTO
        //    {
        //        Id                = request.Id,
        //        HospitalId        = hospital.Id,
        //        HospitalName      = hospital.Name,
        //        BloodTypeId       = bloodType.Id,
        //        BloodTypeName     = bloodType.TypeName,
        //        QuantityRequired  = request.QuantityRequired,
        //        QuantityFulfilled = 0,
        //        UrgencyLevel      = request.UrgencyLevel,
        //        Status            = request.Status,
        //        Note              = request.Note,
        //        Deadline          = request.Deadline,
        //        CreatedAt         = request.DateOfCreattion
        //    };
        //}
        public async Task<BloodRequestResponseDTO> CreateRequestAsync(
        string hospitalAdminUserId,
        CreateBloodRequestDTO dto)
        {
            // 1. Get hospital admin — scalars only to avoid enum cast issues
            var hospitalAdmin = await _context.HospitalAdmins
                .AsNoTracking()
                .Where(ha => ha.UserId == hospitalAdminUserId && !ha.IsDeleted)
                .Select(ha => new { ha.HospitalId })
                .FirstOrDefaultAsync();
            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No active hospital admin record found.");

            // 2. Get hospital — cast Status to int to avoid LazyLoadingProxy error
            var hospital = await _context.Hospitals
                .AsNoTracking()
                .Where(h => h.Id == hospitalAdmin.HospitalId && !h.IsDeleted)
                .Select(h => new
                {
                    h.Id,
                    h.Name,
                    h.Latitude,
                    h.Longitude,
                    StatusInt = (int)h.Status
                })
                .FirstOrDefaultAsync();
            if (hospital == null)
                throw new UnauthorizedAccessException("Associated hospital not found.");

            if (hospital.StatusInt != 2)
                throw new InvalidOperationException("Your hospital must be active to create blood requests.");

            // 3. Validate blood type
            var bloodType = await _context.BloodTypes
                .AsNoTracking()
                .Where(b => b.Id == dto.BloodTypeId)
                .Select(b => new { b.Id, b.TypeName })
                .FirstOrDefaultAsync();
            if (bloodType == null)
                throw new ArgumentException("Invalid blood type selected.");

            // 4. Create the donation request
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

            // 5. Create in-app notification for the hospital admin
            var notification = new Notification
            {
                UserId = hospitalAdminUserId,
                Title = "Blood Request Created",
                Body = $"Your request for {bloodType.TypeName} blood has been submitted successfully and is being synced to the ledger background.",
                IsRead = false,
                RelatedRequestId = request.Id
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // =================================================================
            // BACKGROUND JOBS (ENQUEUED IN HANGFIRE)
            // =================================================================

            // Job A: Log request metadata asynchronously to the blockchain network
            BackgroundJob.Enqueue<LogRequestToBlockchainJob>(
                job => job.ExecuteAsync(request.Id));

            // Job B: Find eligible nearby donors and send out push notifications
            BackgroundJob.Enqueue<FindAndNotifyDonorsJob>(
                job => job.ExecuteAsync(request.Id, dto.MaxDistanceKm));

            // =================================================================

            // 7. Return response
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

            var total       = await q.CountAsync();
            var totalActive = await q.Where(r => r.Status == RequestStatus.Open).CountAsync();

            // ── Main query ────────────────────────────────────────────
            var requests = await q
                .OrderByDescending(r => r.DateOfCreattion)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(r => new BloodRequestSummaryDTO
                {
                    Id                = r.Id,
                    HospitalId        = r.HospitalId,
                    BloodTypeId       = r.BloodTypeId,
                    BloodTypeName     = r.BloodType.TypeName,
                    QuantityRequired  = r.QuantityRequired,
                    QuantityFulfilled = r.QuantityFulfilled,
                    ProgressPercent   = r.QuantityRequired == 0 ? 0
                                            : Math.Round(
                                                (double)r.QuantityFulfilled /
                                                r.QuantityRequired * 100, 1),
                    UrgencyLevel      = r.UrgencyLevel,
                    Status            = r.Status,
                    Note              = r.Note,
                    Deadline          = r.Deadline,
                    CreatedAt         = r.DateOfCreattion
                })
                .ToListAsync();

            return new BloodRequestListResult
            {
                Success    = true,
                Message    = "Requests retrieved successfully.",
                Total      = total,
                TotalActive = totalActive,
                Page       = query.Page,
                Limit      = query.Limit,
                TotalPages = (int)Math.Ceiling((double)total / query.Limit),
                Value      = requests
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
                    BloodTypeName     = r.BloodType.TypeName,
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
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int totalResponses = responseCounts.Sum(x => x.Count);
            int accepted       = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.Accepted)?.Count ?? 0;
            int arrived        = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.Arrived)?.Count  ?? 0;
            int donated        = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.Donated)?.Count  ?? 0;
            int noShow         = responseCounts.FirstOrDefault(x => x.Status == ResponseStatus.NoShow)?.Count   ?? 0;

            // ── Donor responses list ──────────────────────────────────
            var donorResponses = await _context.RequestResponses
                .AsNoTracking()
                .Where(rr => rr.RequestId == requestId)
                .OrderByDescending(rr => rr.RespondedAt)
                .Select(rr => new
                {
                    rr.Id,
                    rr.DonorId,
                    FullName      = rr.Donor.User.FullName,
                    BloodTypeName = rr.Donor.BloodType.TypeName,
                    StatusInt     = (int)rr.Status,
                    rr.RespondedAt
                })
                .ToListAsync();

            var donorResponseDTOs = donorResponses.Select(rr => new DonorResponseDTO
            {
                ResponseId    = rr.Id,
                DonorId       = rr.DonorId,
                FullName      = rr.FullName,
                BloodTypeName = rr.BloodTypeName,
                Status        = ((ResponseStatus)rr.StatusInt).ToString(),
                RespondedAt   = rr.RespondedAt
            }).ToList();

            return new BloodRequestDetailResult
            {
                Success = true,
                Message = "Request retrieved successfully.",
                Value   = new BloodRequestDetailDTO
                {
                    Id                = raw.Id,
                    HospitalId        = raw.HospitalId,
                    BloodTypeId       = raw.BloodTypeId,
                    BloodTypeName     = raw.BloodTypeName,
                    QuantityRequired  = raw.QuantityRequired,
                    QuantityFulfilled = raw.QuantityFulfilled,
                    ProgressPercent   = raw.QuantityRequired == 0 ? 0
                                            : Math.Round(
                                                (double)raw.QuantityFulfilled /
                                                raw.QuantityRequired * 100, 1),
                    UrgencyLevel      = raw.UrgencyLevel,
                    Status            = raw.Status,
                    Note              = raw.Note,
                    Deadline          = raw.Deadline,
                    Latitude          = raw.Latitude,
                    Longitude         = raw.Longitude,
                    CreatedAt         = raw.DateOfCreattion,
                    Responses         = totalResponses,
                    Accepted          = accepted,
                    Arrived           = arrived,
                    Donated           = donated,
                    NoShow            = noShow,
                    DonorResponses    = donorResponseDTOs
                }
            };
        }

        // ── Update ────────────────────────────────────────────────────
        //public async Task<BloodRequestResponseDTO> UpdateRequestAsync(
        //    string hospitalAdminUserId,
        //    string requestId,
        //    UpdateBloodRequestDTO dto)
        //{
        //    var hospitalAdmin = await _context.HospitalAdmins
        //        .FirstOrDefaultAsync(ha =>
        //            ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

        //    if (hospitalAdmin == null)
        //        throw new UnauthorizedAccessException("No hospital admin record found.");

        //    var request = await _context.DonationRequests
        //        .Include(r => r.BloodType)
        //        .FirstOrDefaultAsync(r =>
        //            r.Id == requestId &&
        //            r.HospitalId == hospitalAdmin.HospitalId);

        //    if (request == null)
        //        throw new KeyNotFoundException("Request not found.");

        //    if (request.Status != RequestStatus.Open)
        //        throw new InvalidOperationException(
        //            "Only open requests can be updated.");

        //    // Apply updates — only update fields that are provided
        //    if (dto.QuantityRequired.HasValue)
        //        request.QuantityRequired = dto.QuantityRequired.Value;

        //    if (dto.UrgencyLevel.HasValue)
        //        request.UrgencyLevel = dto.UrgencyLevel.Value;

        //    if (dto.Note is not null)
        //        request.Note = dto.Note;

        //    if (dto.Deadline.HasValue)
        //        request.Deadline = dto.Deadline;

        //    if (dto.Status.HasValue && dto.Status == RequestStatus.Cancelled)
        //        request.Status = RequestStatus.Cancelled;

        //    await _context.SaveChangesAsync();

        //    return new BloodRequestResponseDTO
        //    {
        //        Id                = request.Id,
        //        HospitalId        = request.HospitalId,
        //        HospitalName      = hospitalAdmin.Hospital?.Name ?? "",
        //        BloodTypeId       = request.BloodTypeId,
        //        BloodTypeName     = request.BloodType?.TypeName  ?? "",
        //        QuantityRequired  = request.QuantityRequired,
        //        QuantityFulfilled = request.QuantityFulfilled,
        //        UrgencyLevel      = request.UrgencyLevel,
        //        Status            = request.Status,
        //        Note              = request.Note,
        //        Deadline          = request.Deadline,
        //        CreatedAt         = request.DateOfCreattion
        //    };
        //}
        public async Task<BloodRequestResponseDTO> UpdateRequestAsync(
    string hospitalAdminUserId,
    string requestId,
    UpdateBloodRequestDTO dto)
        {
            var hospitalAdmin = await _context.HospitalAdmins
                .FirstOrDefaultAsync(ha => ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No hospital admin record found.");

            // Retrieve writable entity tracking reference directly without complex Include lookups
            var request = await _context.DonationRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.HospitalId == hospitalAdmin.HospitalId);

            if (request == null)
                throw new KeyNotFoundException("Request not found.");

            if (request.Status != RequestStatus.Open)
                throw new InvalidOperationException("Only open requests can be updated.");

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

            // Save internal mutations to database first
            await _context.SaveChangesAsync();

            // =================================================================
            // ENQUEUE THE BLOCKCHAIN COMBINED BACKGROUND UPDATE JOB
            // =================================================================
            BackgroundJob.Enqueue<UpdateRequestOnBlockchainJob>(
                job => job.ExecuteAsync(request.Id));
            // =================================================================

            // Pull lookup labels optimized without breaking include tracking limitations
            var bloodTypeName = await _context.BloodTypes
                .AsNoTracking()
                .Where(b => b.Id == request.BloodTypeId)
                .Select(b => b.TypeName)
                .FirstOrDefaultAsync() ?? "";

            var hospitalName = await _context.Hospitals
                .AsNoTracking()
                .Where(h => h.Id == hospitalAdmin.HospitalId)
                .Select(h => h.Name)
                .FirstOrDefaultAsync() ?? "";

            return new BloodRequestResponseDTO
            {
                Id = request.Id,
                HospitalId = request.HospitalId,
                HospitalName = hospitalName,
                BloodTypeId = request.BloodTypeId,
                BloodTypeName = bloodTypeName,
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
            if(request.Id != null && request.HospitalId != null)
            BackgroundJob.Enqueue<UpdateRequestOnBlockchainJob>(
               job => job.ExecuteAsync(request.Id));
            return true;
        }
        ///////////
        ///
        public async Task<bool> UpdateDonorResponseStatusAsync(string hospitalAdminUserId, UpdateResponseStatusDTO dto)
        {
            // 1. جلب الـ HospitalId الخاص بالأدمن للتأكد من هويته
            var hospitalAdmin = await _context.HospitalAdmins
                .AsNoTracking()
                .Where(ha => ha.UserId == hospitalAdminUserId && !ha.IsDeleted)
                .Select(ha => new { ha.HospitalId })
                .FirstOrDefaultAsync();

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No active hospital admin record found.");

            // 2. جلب سجل الاستجابة
            var responseLog = await _context.RequestResponses
                .FirstOrDefaultAsync(rr => rr.Id == dto.ResponseId);

            if (responseLog == null)
                throw new ArgumentException("Response record not found.");

            // 3. التأكد التام أن الطلب يخص مستشفى هذا الأدمن بالظبط ( لمنع الـ ID Spoofing )
            var bloodRequest = await _context.DonationRequests
                .FirstOrDefaultAsync(r => r.Id == responseLog.RequestId && r.HospitalId == hospitalAdmin.HospitalId && !r.IsDeleted);

            if (bloodRequest == null)
                throw new UnauthorizedAccessException("You do not have permission to modify this request's responses.");

            // 4. تحديث الحالة
            var oldStatus = responseLog.Status;
            responseLog.Status = (ResponseStatus)dto.NewStatus;
            responseLog.DateOfUpdate = DateTime.UtcNow;

            // [باقي منطق الـ Donated والـ History كما هو لربط الـ Dashboard تلقائياً...]
            if (responseLog.Status == ResponseStatus.Donated && oldStatus != ResponseStatus.Donated)
            {
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Id == responseLog.DonorId);

                // 🛑 شرط الأمان الطبي: التأكد من أنه متاح ومؤهل وبقاله أكتر من 3 شهور متبرعش
                var threeMonthsAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));

                if (donor == null || !donor.IsAvailableForDonation || donor.IsDeleted)
                {
                    throw new InvalidOperationException("This donor is currently ineligible or unavailable for blood donation.");
                }

                if (donor.LastDonationDate.HasValue && donor.LastDonationDate.Value > threeMonthsAgo)
                {
                    var nextAvailableDate = donor.LastDonationDate.Value.AddMonths(3);
                    throw new InvalidOperationException($"This donor cannot donate yet. Next eligible date is after: {nextAvailableDate}");
                }
                bloodRequest.QuantityFulfilled += 1;

                // ب. إذا اكتمل الطلب تماماً، نغير حالته إلى Fulfilled (فرضاً 2 = Fulfilled)
                if (bloodRequest.QuantityFulfilled >= bloodRequest.QuantityRequired)
                {
                    bloodRequest.Status = (RequestStatus)2;
                }

                // ج. تحديث تاريخ آخر تبرع للمتبرع (LastDonationDate) لليوم عشان يبقى غير مؤهل (Ineligible) تلقائياً
                if (donor != null)
                {
                    donor.LastDonationDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }

                // د. إضافة سجل التبرع في الـ DonationHistories عشان الـ Dashboard تسمع فوراً
                var historyExists = await _context.DonationHistories.AnyAsync(h => h.RequestId == bloodRequest.Id && h.DonorId == responseLog.DonorId);
                if (!historyExists)
                {
                    var history = new DonationHistory
                    {
                        Id = Guid.NewGuid().ToString(),
                        DonorId = responseLog.DonorId,
                        HospitalId = hospitalAdmin.HospitalId,
                        RequestId = bloodRequest.Id,
                        DonationDate = DateTime.UtcNow,
                        AmountML = 450, // الكمية القياسية للتبرع بالدم
                        DateOfCreattion = DateTime.UtcNow,
                        DateOfUpdate = DateTime.UtcNow
                    };
                    _context.DonationHistories.Add(history);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
