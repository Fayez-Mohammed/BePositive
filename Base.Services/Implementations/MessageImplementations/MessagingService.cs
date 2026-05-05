// Base.Services/Implementations/HospitalImplementations/MessagingService.cs

using Base.DAL.Contexts;
using Base.DAL.Models.MessagingModels;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.MessagingDTOs;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class MessagingService : IMessagingService
    {
        private readonly AppDbContext _context;

        public MessagingService(AppDbContext context)
        {
            _context = context;
        }

        // ── Helper: get hospitalId from admin user ────────────────
        private async Task<string> GetHospitalIdAsync(string hospitalAdminUserId)
        {
            var admin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (admin == null)
                throw new UnauthorizedAccessException(
                    "No hospital admin record found.");

            return admin.HospitalId;
        }

        // ── Helper: get donorId from donor user ───────────────────
        private async Task<string> GetDonorIdAsync(string donorUserId)
        {
            var donor = await _context.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.UserId == donorUserId && !d.IsDeleted);

            if (donor == null)
                throw new UnauthorizedAccessException(
                    "Donor record not found.");

            return donor.Id;
        }

        // ── Helper: map Conversation entity → DTO ─────────────────
        private static ConversationDTO MapConversation(
            Conversation c,
            string donorFullName,
            string? bloodTypeName,
            string? phone,
            string? city,
            bool online)
        {
            return new ConversationDTO
            {
                ConversationId = c.Id,
                DonorId        = c.DonorId,
                FullName       = donorFullName,
                BloodTypeName  = bloodTypeName,
                Phone          = phone,
                City           = city,
                Online         = online,
                LastMessage    = c.LastMessage,
                LastMessageAt  = c.LastMessageAt,
                UnreadCount    = c.HospitalUnreadCount
            };
        }

        // ── Helper: map ChatMessage entity → DTO ──────────────────
        private static MessageDTO MapMessage(ChatMessage m)
        {
            return new MessageDTO
            {
                MessageId      = m.Id,
                ConversationId = m.ConversationId,
                SenderId       = m.SenderId,
                SenderType     = m.SenderType,
                Text           = m.IsDeleted ? null : m.Text,
                Status         = m.Status,
                IsDeleted      = m.IsDeleted,
                CreatedAt      = m.CreatedAt,
                EditedAt       = m.EditedAt
            };
        }

        // ── GET /hospital/conversations ───────────────────────────
        public async Task<ConversationListResult> GetConversationsAsync(
            string hospitalAdminUserId,
            GetConversationsQuery query)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            // Pull raw conversation data — scalars only
            var q = _context.Conversations
                .AsNoTracking()
                .Where(c => c.HospitalId == hospitalId)
                .AsQueryable();

            // Search by donor name
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(c =>
                    _context.Donors
                        .Where(d => d.Id == c.DonorId)
                        .Select(d => d.User.FullName)
                        .FirstOrDefault()!
                        .ToLower()
                        .Contains(search));
            }

            var total = await q.CountAsync();

            var conversations = await q
                .OrderByDescending(c => c.LastMessageAt)
                .ThenByDescending(c => c.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new
                {
                    c.Id,
                    c.DonorId,
                    c.HospitalId,
                    c.LastMessage,
                    c.LastMessageAt,
                    c.HospitalUnreadCount,
                    DonorFullName  = _context.Donors
                        .Where(d => d.Id == c.DonorId)
                        .Select(d => d.User.FullName)
                        .FirstOrDefault(),
                    BloodTypeName  = _context.Donors
                        .Where(d => d.Id == c.DonorId)
                        .Select(d => d.BloodType.TypeName)
                        .FirstOrDefault(),
                    Phone = _context.Donors
                        .Where(d => d.Id == c.DonorId)
                        .Select(d => d.User.PhoneNumber)
                        .FirstOrDefault(),
                    City = _context.Donors
                        .Where(d => d.Id == c.DonorId && d.City != null)
                        .Select(d => d.City!.NameEn)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var items = conversations.Select(c => new ConversationDTO
            {
                ConversationId = c.Id,
                DonorId        = c.DonorId,
                FullName       = c.DonorFullName ?? "Unknown",
                BloodTypeName  = c.BloodTypeName,
                Phone          = c.Phone,
                City           = c.City,
                Online         = false, // overridden by SignalR PresenceTracker on the client
                LastMessage    = c.LastMessage,
                LastMessageAt  = c.LastMessageAt,
                UnreadCount    = c.HospitalUnreadCount
            }).ToList();

            return new ConversationListResult
            {
                Success    = true,
                Message    = "Conversations retrieved successfully.",
                Items      = items,
                Total      = total,
                Page       = query.Page,
                PageSize   = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
            };
        }

        // ── POST /hospital/conversations ──────────────────────────
        public async Task<ConversationResult> StartOrGetConversationAsync(
            string hospitalAdminUserId,
            string donorId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            // Validate donor exists
            var donorInfo = await _context.Donors
                .AsNoTracking()
                .Where(d => d.Id == donorId && !d.IsDeleted)
                .Select(d => new
                {
                    d.Id,
                    FullName      = d.User.FullName,
                    BloodTypeName = d.BloodType.TypeName,
                    Phone         = d.User.PhoneNumber,
                    City          = d.City != null ? d.City.NameEn : null
                })
                .FirstOrDefaultAsync();

            if (donorInfo == null)
                throw new KeyNotFoundException("Donor not found.");

            // Get existing or create new
            var existing = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.HospitalId == hospitalId &&
                    c.DonorId    == donorId);

            if (existing != null)
            {
                return new ConversationResult
                {
                    Success = true,
                    Message = "Conversation already exists.",
                    Value   = new ConversationDTO
                    {
                        ConversationId = existing.Id,
                        DonorId        = existing.DonorId,
                        FullName       = donorInfo.FullName,
                        BloodTypeName  = donorInfo.BloodTypeName,
                        Phone          = donorInfo.Phone,
                        City           = donorInfo.City,
                        Online         = false,
                        LastMessage    = existing.LastMessage,
                        LastMessageAt  = existing.LastMessageAt,
                        UnreadCount    = existing.HospitalUnreadCount
                    }
                };
            }

            var conversation = new Conversation
            {
                HospitalId = hospitalId,
                DonorId    = donorId,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return new ConversationResult
            {
                Success = true,
                Message = "Conversation started.",
                Value   = new ConversationDTO
                {
                    ConversationId = conversation.Id,
                    DonorId        = conversation.DonorId,
                    FullName       = donorInfo.FullName,
                    BloodTypeName  = donorInfo.BloodTypeName,
                    Phone          = donorInfo.Phone,
                    City           = donorInfo.City,
                    Online         = false,
                    LastMessage    = null,
                    LastMessageAt  = null,
                    UnreadCount    = 0
                }
            };
        }

        // ── GET /hospital/conversations/:id/messages ──────────────
        public async Task<MessageListResult> GetMessagesAsync(
            string hospitalAdminUserId,
            string conversationId,
            GetMessagesQuery query)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            // Verify this conversation belongs to this hospital
            var exists = await _context.Conversations
                .AsNoTracking()
                .AnyAsync(c =>
                    c.Id         == conversationId &&
                    c.HospitalId == hospitalId);

            if (!exists)
                throw new KeyNotFoundException("Conversation not found.");

            var q = _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .AsQueryable();

            // Cursor-based pagination: load messages older than cursor
            if (!string.IsNullOrWhiteSpace(query.Cursor))
            {
                var cursorMsg = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.Id == query.Cursor)
                    .Select(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                if (cursorMsg != default)
                    q = q.Where(m => m.CreatedAt < cursorMsg);
            }

            var messages = await q
                .OrderByDescending(m => m.CreatedAt)
                .Take(query.Limit + 1) // take one extra to check if more exist
                .Select(m => new
                {
                    m.Id,
                    m.ConversationId,
                    m.SenderId,
                    m.SenderType,
                    m.Text,
                    m.Status,
                    m.IsDeleted,
                    m.CreatedAt,
                    m.EditedAt
                })
                .ToListAsync();

            // Check if there are more pages
            bool hasMore = messages.Count > query.Limit;
            if (hasMore) messages = messages.Take(query.Limit).ToList();

            var items = messages.Select(m => new MessageDTO
            {
                MessageId      = m.Id,
                ConversationId = m.ConversationId,
                SenderId       = m.SenderId,
                SenderType     = m.SenderType,
                Text           = m.IsDeleted ? null : m.Text,
                Status         = m.Status,
                IsDeleted      = m.IsDeleted,
                CreatedAt      = m.CreatedAt,
                EditedAt       = m.EditedAt
            })
            .OrderBy(m => m.CreatedAt) // return in ascending order for UI
            .ToList();

            return new MessageListResult
            {
                Success    = true,
                Message    = "Messages retrieved successfully.",
                Items      = items,
                NextCursor = hasMore ? messages.Last().Id : null
            };
        }

        // ── POST /hospital/conversations/:id/messages ─────────────
        public async Task<MessageResult> SendMessageAsync(
            string hospitalAdminUserId,
            string conversationId,
            SendMessageDTO dto)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id         == conversationId &&
                    c.HospitalId == hospitalId);

            if (conversation == null)
                throw new KeyNotFoundException("Conversation not found.");

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId       = hospitalAdminUserId,
                SenderType     = "Hospital",
                Text           = dto.Text,
                Status         = "Sent",
                CreatedAt      = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);

            // Update conversation cache
            conversation.LastMessage   = dto.Text.Length > 60
                ? dto.Text[..60] + "…"
                : dto.Text;
            conversation.LastMessageAt = message.CreatedAt;
            conversation.UpdatedAt     = message.CreatedAt;

            await _context.SaveChangesAsync();

            return new MessageResult
            {
                Success = true,
                Message = "Message sent.",
                Value   = MapMessage(message)
            };
        }

        // ── DELETE /hospital/messages/:messageId ──────────────────
        public async Task<bool> DeleteMessageAsync(
            string hospitalAdminUserId,
            string messageId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            var message = await _context.ChatMessages
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m =>
                    m.Id              == messageId &&
                    m.SenderId        == hospitalAdminUserId &&
                    m.Conversation.HospitalId == hospitalId &&
                    !m.IsDeleted);

            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            message.IsDeleted = true;
            message.EditedAt  = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── POST /hospital/conversations/:id/read ─────────────────
        public async Task<bool> MarkConversationReadAsync(
            string hospitalAdminUserId,
            string conversationId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id         == conversationId &&
                    c.HospitalId == hospitalId);

            if (conversation == null)
                throw new KeyNotFoundException("Conversation not found.");

            // Reset unread count
            conversation.HospitalUnreadCount = 0;

            // Mark all incoming donor messages as Read
            var unreadMessages = await _context.ChatMessages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderType     == "Donor"        &&
                    m.Status         != "Read")
                .ToListAsync();

            foreach (var m in unreadMessages)
                m.Status = "Read";

            await _context.SaveChangesAsync();
            return true;
        }

        // ── GET /hospital/messages/unread-count ───────────────────
        public async Task<UnreadCountResult> GetUnreadCountAsync(
            string hospitalAdminUserId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            var total = await _context.Conversations
                .AsNoTracking()
                .Where(c => c.HospitalId == hospitalId)
                .SumAsync(c => (int?)c.HospitalUnreadCount) ?? 0;

            return new UnreadCountResult
            {
                Success = true,
                Message = "Unread count retrieved.",
                Value   = new UnreadCountDTO { Total = total }
            };
        }

        // ── Hub helper: conversationId from donorId + hospitalId ──
        public async Task<string?> GetConversationIdForDonorAsync(
            string donorId,
            string hospitalId)
        {
            return await _context.Conversations
                .AsNoTracking()
                .Where(c =>
                    c.DonorId    == donorId &&
                    c.HospitalId == hospitalId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
        }

        // ── Hub helper: donor sends a message ─────────────────────
        public async Task<MessageResult> SendDonorMessageAsync(
            string donorUserId,
            string conversationId,
            string text)
        {
            var donorId = await GetDonorIdAsync(donorUserId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id      == conversationId &&
                    c.DonorId == donorId);

            if (conversation == null)
                throw new KeyNotFoundException("Conversation not found.");

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId       = donorUserId,
                SenderType     = "Donor",
                Text           = text,
                Status         = "Sent",
                CreatedAt      = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);

            // Update conversation cache + hospital unread count
            conversation.LastMessage         = text.Length > 60 ? text[..60] + "…" : text;
            conversation.LastMessageAt       = message.CreatedAt;
            conversation.UpdatedAt           = message.CreatedAt;
            conversation.HospitalUnreadCount += 1;

            await _context.SaveChangesAsync();

            return new MessageResult
            {
                Success = true,
                Message = "Message sent.",
                Value   = MapMessage(message)
            };
        }

        // ── Hub helper: donor marks messages read ──────────────────
        public async Task<bool> MarkConversationReadByDonorAsync(
            string donorUserId,
            string conversationId)
        {
            var donorId = await GetDonorIdAsync(donorUserId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id      == conversationId &&
                    c.DonorId == donorId);

            if (conversation == null) return false;

            var unreadMessages = await _context.ChatMessages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderType     == "Hospital"     &&
                    m.Status         != "Read")
                .ToListAsync();

            foreach (var m in unreadMessages)
                m.Status = "Read";

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
