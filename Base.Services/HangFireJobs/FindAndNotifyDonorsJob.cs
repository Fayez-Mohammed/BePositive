// Base.Services/HangfireJobs/FindAndNotifyDonorsJob.cs

using Base.DAL.Contexts;
using Base.DAL.Models.RequestModels;
using Base.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Base.Services.HangfireJobs
{
    public class FindAndNotifyDonorsJob
    {
        private readonly AppDbContext _context;
        private readonly IFcmService _fcmService;
        private readonly ILogger<FindAndNotifyDonorsJob> _logger;

        private const int CooldownDays = 56;

        public FindAndNotifyDonorsJob(
            AppDbContext context,
            IFcmService fcmService,
            ILogger<FindAndNotifyDonorsJob> logger)
        {
            _context = context;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task ExecuteAsync(string requestId)
        {
            _logger.LogInformation(
                "FindAndNotifyDonorsJob started for RequestId: {RequestId}", requestId);

            // ── 1. Load the request (scalars only — no navigation) ────
            var raw = await _context.DonationRequests
                .AsNoTracking()
                .Where(r => r.Id == requestId && !r.IsDeleted)
                .Select(r => new
                {
                    r.Id,
                    r.HospitalId,
                    r.BloodTypeId,
                    StatusInt = (int)r.Status,
                    UrgencyLevelInt = (int)r.UrgencyLevel
                })
                .FirstOrDefaultAsync();

            if (raw == null)
            {
                _logger.LogWarning(
                    "Request {RequestId} not found. Job aborted.", requestId);
                return;
            }

            // Only process Open requests (RequestStatus.Open = 1)
            if (raw.StatusInt != 1)
            {
                _logger.LogInformation(
                    "Request {RequestId} is not Open. Job skipped.", requestId);
                return;
            }

            // ── 2. Get compatible blood type IDs ──────────────────────
            var compatibleBloodTypeIds = await _context.BloodTypeCompatibilities
                .AsNoTracking()
                .Where(c => c.RecipientBloodTypeId == raw.BloodTypeId)
                .Select(c => c.DonorBloodTypeId)
                .ToListAsync();

            // Always include exact match
            if (!compatibleBloodTypeIds.Contains(raw.BloodTypeId))
                compatibleBloodTypeIds.Add(raw.BloodTypeId);

            // ── 3. Get eligible donors that have an FCM token ─────────
            var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-CooldownDays);

            var eligibleDonors = await _context.Donors
                .AsNoTracking()
                .Where(d =>
                    !d.IsDeleted &&
                    d.IsAvailableForDonation &&
                    d.FcmToken != null &&
                    compatibleBloodTypeIds.Contains(d.BloodTypeId) &&
                    (d.LastDonationDate == null ||
                     d.LastDonationDate < cutoff))
                .Select(d => new
                {
                    d.Id,
                    d.FcmToken
                })
                .ToListAsync();

            _logger.LogInformation(
                "{Count} eligible donors found.", eligibleDonors.Count);

            if (!eligibleDonors.Any())
            {
                _logger.LogInformation(
                    "No eligible donors for request {RequestId}.", requestId);
                return;
            }

            // ── 4. Skip donors already notified for this request ──────
            var alreadyNotifiedList = await _context.DonorNotificationLogs
                .AsNoTracking()
                .Where(l => l.RequestId == requestId)
                .Select(l => l.DonorId)
                .ToListAsync();

            var alreadyNotified = alreadyNotifiedList.ToHashSet();

            var toNotify = eligibleDonors
                .Where(d => !alreadyNotified.Contains(d.Id))
                .ToList();

            if (!toNotify.Any())
            {
                _logger.LogInformation(
                    "All eligible donors already notified for request {RequestId}.", requestId);
                return;
            }

            // ── 5. Build notification content ─────────────────────────
            var bloodTypeName = await _context.BloodTypes
                .AsNoTracking()
                .Where(b => b.Id == raw.BloodTypeId)
                .Select(b => b.TypeName)
                .FirstOrDefaultAsync() ?? raw.BloodTypeId;

            var urgencyLabel = raw.UrgencyLevelInt switch
            {
                3 => "🚨 CRITICAL",
                2 => "⚠️ Urgent",
                _ => "Blood Needed"
            };

            var title = $"{urgencyLabel} — {bloodTypeName} Blood Request";
            var body = $"A hospital near you needs {bloodTypeName} blood donors. Tap to respond.";

            var data = new Dictionary<string, string>
            {
                { "requestId",   requestId       },
                { "bloodTypeId", raw.BloodTypeId  },
                { "type",        "blood_request"  }
            };

            // ── 6. Send FCM batch ─────────────────────────────────────
            var tokens = toNotify
                .Select(d => d.FcmToken!)
                .ToList();

            await _fcmService.SendBatchNotificationsAsync(tokens, title, body, data);

            // ── 7. Log each notification in DonorNotificationLogs ─────
            var sentAt = DateTime.UtcNow;

            var logs = toNotify.Select(d => new DonorNotificationLog
            {
                DonorId = d.Id,
                RequestId = requestId,
                Channel = "Push",
                SentAt = sentAt
            }).ToList();

            _context.DonorNotificationLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "FindAndNotifyDonorsJob completed. Notified {Count} donors for request {RequestId}.",
                toNotify.Count, requestId);
        }
    }
}