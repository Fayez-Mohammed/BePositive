// Base.Services/Implementations/DonorImplementations/DonorMessagingService.cs
using Base.DAL.Contexts;
//using Base.Services.Interfaces.DonorInterfaces;
using Base.Services.Interfaces.MessagesInterfaces;
using Base.Shared.DTOs.MessagingDTOs;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.DonorImplementations
{
    public class DonorMessagingService : IDonorMessagingService
    {
        private readonly AppDbContext _context;

        public DonorMessagingService(AppDbContext context)
        {
            _context = context;
        }

        // دالة مساعدة لجلب الـ DonorId الفرعي من الـ Identity UserId
        private async Task<string> GetDonorIdAsync(string donorUserId)
        {
            var donor = await _context.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == donorUserId && !d.IsDeleted);

            if (donor == null)
                throw new UnauthorizedAccessException("Donor record not found.");

            return donor.Id;
        }

        // جلب محادثات المتبرع
        public async Task<DonorConversationListResult> GetConversationsAsync(
            string donorUserId,
            GetConversationsQuery query)
        {
            var donorId = await GetDonorIdAsync(donorUserId);

            var q = _context.Conversations
                .AsNoTracking()
                .Where(c => c.DonorId == donorId)
                .AsQueryable();

            // البحث باسم المستشفى
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(c => _context.Hospitals
                    .Where(h => h.Id == c.HospitalId)
                    .Select(h => h.Name)
                    .FirstOrDefault()!
                    .ToLower()
                    .Contains(search));
            }

            var total = await q.CountAsync();

            var conversations = await q
                .OrderByDescending(c => c.LastMessageAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new
                {
                    c.Id,
                    c.HospitalId,
                    c.LastMessage,
                    c.LastMessageAt,
                    HospitalName = _context.Hospitals.Where(h => h.Id == c.HospitalId).Select(h => h.Name).FirstOrDefault(),
                    HospitalCity = _context.Hospitals.Where(h => h.Id == c.HospitalId && h.City != null).Select(h => h.City!.NameEn).FirstOrDefault(),
                    DonorUnreadCount = _context.ChatMessages.Count(m => m.ConversationId == c.Id && m.SenderType == "Hospital" && m.Status != "Read")
                })
                .ToListAsync();

            var items = conversations.Select(c => new DonorConversationDTO
            {
                ConversationId = c.Id,
                HospitalId = c.HospitalId,
                HospitalName = c.HospitalName ?? "Unknown Hospital",
                HospitalCity = c.HospitalCity,
                Online = false, // بيحصل لها لايف تفصيل بالـ SignalR
                LastMessage = c.LastMessage,
                LastMessageAt = c.LastMessageAt,
                UnreadCount = c.DonorUnreadCount
            }).ToList();

            return new DonorConversationListResult
            {
                Success = true,
                Message = "Donor conversations retrieved successfully.",
                Items = items,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
            };
        }

        // جلب رسائل محادثة معينة للمتبرع مع حماية الـ Security
        public async Task<DonorMessageListResult> GetMessagesAsync(
            string donorUserId,
            string conversationId,
            GetMessagesQuery query)
        {
            var donorId = await GetDonorIdAsync(donorUserId);

            // تأمين الحماية: فحص ملكية المحادثة
            var exists = await _context.Conversations
                .AsNoTracking()
                .AnyAsync(c => c.Id == conversationId && c.DonorId == donorId);

            if (!exists)
                throw new KeyNotFoundException("Conversation not found or access denied.");

            var q = _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .AsQueryable();

            // Cursor Pagination لزوم الـ Infinite scroll في الفلوتر
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
                .Take(query.Limit + 1)
                .ToListAsync();

            bool hasMore = messages.Count > query.Limit;
            if (hasMore) messages = messages.Take(query.Limit).ToList();

            var items = messages.Select(m => new DonorMessageDTO
            {
                MessageId = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                SenderType = m.SenderType,
                Text = m.IsDeleted ? null : m.Text,
                Status = m.Status,
                IsDeleted = m.IsDeleted,
                CreatedAt = m.CreatedAt
            })
            .OrderBy(m => m.CreatedAt)
            .ToList();

            return new DonorMessageListResult
            {
                Success = true,
                Message = "Messages retrieved successfully.",
                Items = items,
                NextCursor = hasMore ? messages.Last().Id : null
            };
        }
    }
}