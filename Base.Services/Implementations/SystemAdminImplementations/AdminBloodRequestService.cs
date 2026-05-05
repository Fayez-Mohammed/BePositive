// Base.Services/Implementations/AdminBloodRequestService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations
{
    public class AdminBloodRequestService : IAdminBloodRequestService
    {
        private readonly AppDbContext _context;

        public AdminBloodRequestService(AppDbContext context)
        {
            _context = context;
        }

        // ── GET /api/admin/requests/stats ─────────────────────
        public async Task<AdminBloodRequestStatsDTO> GetStatsAsync()
        {
            var totalRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r => !r.IsDeleted);

            var openRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    !r.IsDeleted &&
                    r.Status == RequestStatus.Open);

            var fulfilledRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    !r.IsDeleted &&
                    r.Status == RequestStatus.Fulfilled);

            var criticalRequests = await _context.DonationRequests
                .AsNoTracking()
                .CountAsync(r =>
                    !r.IsDeleted &&
                    r.Status == RequestStatus.Open &&
                    r.UrgencyLevel == UrgencyLevel.Critical);

            var totalResponses = await _context.RequestResponses
                .AsNoTracking()
                .CountAsync();

            var totalDonations = await _context.DonationHistories
                .AsNoTracking()
                .CountAsync();

            return new AdminBloodRequestStatsDTO
            {
                TotalRequests     = totalRequests,
                OpenRequests      = openRequests,
                FulfilledRequests = fulfilledRequests,
                CriticalRequests  = criticalRequests,
                TotalResponses    = totalResponses,
                TotalDonations    = totalDonations
            };
        }

        // ── GET /api/admin/requests ───────────────────────────
        public async Task<AdminBloodRequestListResult> GetAllRequestsAsync(
            string? hospitalId,
            string? bloodTypeId,
            string? status,
            string? urgencyLevel,
            string? search,
            int     page,
            int     limit)
        {
            var query = _context.DonationRequests
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(hospitalId))
                query = query.Where(r => r.HospitalId == hospitalId);

            if (!string.IsNullOrWhiteSpace(bloodTypeId))
                query = query.Where(r => r.BloodTypeId == bloodTypeId);

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<RequestStatus>(status, true, out var reqStatus))
                query = query.Where(r => r.Status == reqStatus);

            if (!string.IsNullOrWhiteSpace(urgencyLevel) &&
                Enum.TryParse<UrgencyLevel>(urgencyLevel, true, out var urgency))
                query = query.Where(r => r.UrgencyLevel == urgency);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r =>
                    r.Hospital.Name.ToLower().Contains(s) ||
                    r.BloodType.TypeName.ToLower().Contains(s) ||
                    (r.Note != null && r.Note.ToLower().Contains(s)));
            }

            var total = await query.CountAsync();

            // Get requests with cast to avoid enum error
            var rawRequests = await query
                .OrderByDescending(r => r.DateOfCreattion)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(r => new
                {
                    r.Id,
                    r.HospitalId,
                    HospitalName       = r.Hospital.Name,
                    HospitalCity       = r.Hospital.City != null
                                         ? r.Hospital.City.NameEn : null,
                    HospitalGovernorate = r.Hospital.City != null &&
                                          r.Hospital.City.Governorate != null
                                          ? r.Hospital.City.Governorate.NameEn
                                          : null,
                    r.BloodTypeId,
                    BloodTypeName      = r.BloodType.TypeName,
                    r.QuantityRequired,
                    r.QuantityFulfilled,
                    UrgencyLevelInt    = (int)r.UrgencyLevel,
                    StatusInt          = (int)r.Status,
                    r.Deadline,
                    r.DateOfCreattion
                })
                .ToListAsync();

            // Get response counts per request
            var requestIds = rawRequests.Select(r => r.Id).ToList();
            var responseCounts = await _context.RequestResponses
                .AsNoTracking()
                .Where(rr => requestIds.Contains(rr.RequestId))
                .GroupBy(rr => new { rr.RequestId, rr.Status })
                .Select(g => new
                {
                    g.Key.RequestId,
                    StatusInt = (int)g.Key.Status,
                    Count     = g.Count()
                })
                .ToListAsync();

            var responseDict = responseCounts
                .GroupBy(rc => rc.RequestId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.StatusInt, x => x.Count));

            // Map to DTOs
            var requests = rawRequests.Select(r =>
            {
                var responseData = responseDict
                    .TryGetValue(r.Id, out var counts)
                    ? counts
                    : new Dictionary<int, int>();

                var totalResponses = responseData.Values.Sum();
                var donated = responseData
                    .TryGetValue((int)ResponseStatus.Donated, out var d)
                    ? d : 0;

                return new AdminBloodRequestSummaryDTO
                {
                    RequestId         = r.Id,
                    HospitalId        = r.HospitalId,
                    HospitalName      = r.HospitalName,
                    HospitalCity      = r.HospitalCity,
                    HospitalGovernorate = r.HospitalGovernorate,
                    BloodTypeId       = r.BloodTypeId,
                    BloodTypeName     = r.BloodTypeName,
                    QuantityRequired  = r.QuantityRequired,
                    QuantityFulfilled = r.QuantityFulfilled,
                    ProgressPercent   = r.QuantityRequired == 0 ? 0
                        : Math.Round((double)r.QuantityFulfilled /
                                     r.QuantityRequired * 100, 1),
                    UrgencyLevel      = ((UrgencyLevel)r.UrgencyLevelInt).ToString(),
                    Status            = ((RequestStatus)r.StatusInt).ToString(),
                    Deadline          = (DateTime)r.Deadline,
                    CreatedAt         = r.DateOfCreattion,
                    TotalResponses    = totalResponses,
                    DonatedCount      = donated
                };
            }).ToList();

            return new AdminBloodRequestListResult
            {
                Success    = true,
                Message    = "Requests retrieved successfully.",
                Total      = total,
                Page       = page,
                Limit      = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit),
                Value      = requests
            };
        }

        // ── GET /api/admin/requests/{id} ──────────────────────
        public async Task<AdminBloodRequestDetailResult> GetRequestByIdAsync(
            string requestId)
        {
            var raw = await _context.DonationRequests
                .AsNoTracking()
                .Where(r => r.Id == requestId && !r.IsDeleted)
                .Select(r => new
                {
                    r.Id,
                    r.HospitalId,
                    HospitalName       = r.Hospital.Name,
                    HospitalPhone      = r.Hospital.Phone,
                    HospitalEmail      = r.Hospital.Email,
                    HospitalAddress    = r.Hospital.Address,
                    HospitalCity       = r.Hospital.City != null
                                         ? r.Hospital.City.NameEn : null,
                    HospitalGovernorate = r.Hospital.City != null &&
                                          r.Hospital.City.Governorate != null
                                          ? r.Hospital.City.Governorate.NameEn
                                          : null,
                    r.BloodTypeId,
                    BloodTypeName      = r.BloodType.TypeName,
                    r.QuantityRequired,
                    r.QuantityFulfilled,
                    UrgencyLevelInt    = (int)r.UrgencyLevel,
                    StatusInt          = (int)r.Status,
                    r.Note,
                    r.Deadline,
                    r.Latitude,
                    r.Longitude,
                    r.DateOfCreattion
                })
                .FirstOrDefaultAsync();

            if (raw == null)
                return new AdminBloodRequestDetailResult
                {
                    Success = false,
                    Message = "Request not found."
                };

            // Get response stats
            var responseCounts = await _context.RequestResponses
                .AsNoTracking()
                .Where(rr => rr.RequestId == requestId)
                .GroupBy(rr => rr.Status)
                .Select(g => new
                {
                    StatusInt = (int)g.Key,
                    Count     = g.Count()
                })
                .ToListAsync();

            var countDict = responseCounts
                .ToDictionary(x => x.StatusInt, x => x.Count);

            int totalResponses = countDict.Values.Sum();
            int accepted = countDict
                .TryGetValue((int)ResponseStatus.Accepted, out var a) ? a : 0;
            int arrived = countDict
                .TryGetValue((int)ResponseStatus.Arrived, out var ar) ? ar : 0;
            int donated = countDict
                .TryGetValue((int)ResponseStatus.Donated, out var d) ? d : 0;
            int noShow = countDict
                .TryGetValue((int)ResponseStatus.NoShow, out var ns) ? ns : 0;
            int rejected = countDict
                .TryGetValue((int)ResponseStatus.Rejected, out var r) ? r : 0;

            // Get donor responses
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
                    Phone         = rr.Donor.User.PhoneNumber,
                    StatusInt     = (int)rr.Status,
                    rr.RespondedAt,
                    rr.DonorDistanceKm
                })
                .ToListAsync();

            var donorResponseDTOs = donorResponses.Select(rr =>
                new AdminDonorResponseDTO
                {
                    ResponseId    = rr.Id,
                    DonorId       = rr.DonorId,
                    FullName      = rr.FullName,
                    BloodTypeName = rr.BloodTypeName,
                    Phone         = rr.Phone,
                    Status        = ((ResponseStatus)rr.StatusInt).ToString(),
                    RespondedAt   = rr.RespondedAt,
                    DistanceKm    =(double) rr.DonorDistanceKm
                }).ToList();

            return new AdminBloodRequestDetailResult
            {
                Success = true,
                Message = "Request retrieved successfully.",
                Value   = new AdminBloodRequestDetailDTO
                {
                    RequestId         = raw.Id,
                    HospitalId        = raw.HospitalId,
                    HospitalName      = raw.HospitalName,
                    HospitalPhone     = raw.HospitalPhone,
                    HospitalEmail     = raw.HospitalEmail,
                    HospitalAddress   = raw.HospitalAddress,
                    HospitalCity      = raw.HospitalCity,
                    HospitalGovernorate = raw.HospitalGovernorate,
                    BloodTypeId       = raw.BloodTypeId,
                    BloodTypeName     = raw.BloodTypeName,
                    QuantityRequired  = raw.QuantityRequired,
                    QuantityFulfilled = raw.QuantityFulfilled,
                    ProgressPercent   = raw.QuantityRequired == 0 ? 0
                        : Math.Round((double)raw.QuantityFulfilled /
                                     raw.QuantityRequired * 100, 1),
                    UrgencyLevel      = ((UrgencyLevel)raw.UrgencyLevelInt).ToString(),
                    Status            = ((RequestStatus)raw.StatusInt).ToString(),
                    Note              = raw.Note,
                    Deadline          = (DateTime)raw.Deadline,
                    Latitude          = raw.Latitude,
                    Longitude         = raw.Longitude,
                    CreatedAt         = raw.DateOfCreattion,
                    TotalResponses    = totalResponses,
                    AcceptedCount     = accepted,
                    ArrivedCount      = arrived,
                    DonatedCount      = donated,
                    NoShowCount       = noShow,
                    RejectedCount     = rejected,
                    DonorResponses    = donorResponseDTOs
                }
            };
        }

        public async Task<AllHospitalsResult> GetHospitalsAsync()
        {
            var hospitals = await _context.Hospitals
                .AsNoTracking()
                .Where(h => !h.IsDeleted && h.Status == HospitalStatus.Active)
                .OrderBy(h => h.Name)
                .Select(h => new AllHospitalsDTO
                {
                    Id = h.Id,
                    Name = h.Name
                })
                .ToListAsync();

            return new AllHospitalsResult
            {
                Success = true,
                Message = "Hospitals retrieved successfully.",
                Value = hospitals
            };
        }
    }
}
