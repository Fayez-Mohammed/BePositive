// Base.API/Hubs/MessagingHub.cs
//
// SignalR Hub for real-time messaging.
//
// HOW GROUPS WORK:
//   Each conversation has its own SignalR group: "conv_{conversationId}"
//   When any party (hospital or donor) connects, they join all their conversation groups.
//   When a message is sent, the server broadcasts to the group so all parties receive it.
//
// CLIENT EVENTS (server → client):
//   "message:new"          → NewMessagePayload
//   "message:deleted"      → MessageDeletedPayload
//   "conversation:typing"  → TypingPayload
//   "conversation:read"    → ReadPayload
//   "presence:update"      → PresencePayload
//
// CLIENT → SERVER (hub methods):
//   SendMessage(conversationId, text)
//   DeleteMessage(messageId, conversationId)
//   Typing(conversationId, isTyping)
//   MarkRead(conversationId)

using Base.DAL.Contexts;
using Base.Services.Implementations;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.MessagingDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Base.API.Hubs
{
    [Authorize]
    public class MessagingHub : Hub
    {
        private readonly IMessagingService _messaging;
        private readonly PresenceTracker   _presence;
        private readonly AppDbContext      _context;

        public MessagingHub(
            IMessagingService messaging,
            PresenceTracker presence,
            AppDbContext context)
        {
            _messaging = messaging;
            _presence  = presence;
            _context   = context;
        }

        // ── Helpers ───────────────────────────────────────────────
        private string UserId =>
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("Unauthorized.");

        private string UserType =>
            Context.User?.FindFirstValue(ClaimTypes.Role)
            ?? throw new HubException("Unauthorized.");

        private static string GroupName(string conversationId) =>
            $"conv_{conversationId}";

        // ── OnConnectedAsync ──────────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            var userId   = UserId;
            var userType = UserType;

            _presence.UserConnected(userId, Context.ConnectionId);

            // Join all conversation groups this user is part of
            if (userType == nameof(UserTypes.HospitalAdmin))
            {
                await JoinHospitalGroupsAsync(userId);
            }
            else if (userType == nameof(UserTypes.Donor))
            {
                await JoinDonorGroupsAsync(userId);

                // Notify hospitals that this donor is online
                await BroadcastDonorPresenceAsync(userId, online: true);
            }

            await base.OnConnectedAsync();
        }

        // ── OnDisconnectedAsync ───────────────────────────────────
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId   = UserId;
            var userType = UserType;

            bool fullyOffline = _presence.UserDisconnected(userId, Context.ConnectionId);

            if (fullyOffline && userType == nameof(UserTypes.Donor))
            {
                await BroadcastDonorPresenceAsync(userId, online: false);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ── CLIENT METHOD: SendMessage ────────────────────────────
        public async Task SendMessage(string conversationId, string text, string? attachmentId = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new HubException("Message text cannot be empty.");

            if (text.Length > 2000)
                throw new HubException("Message too long.");

            MessageResult result;

            if (UserType == nameof(UserTypes.HospitalAdmin))
            {
                result = await _messaging.SendMessageAsync(
                    UserId,
                    conversationId,
                    new SendMessageDTO { Text = text });
            }
            else if (UserType == nameof(UserTypes.Donor))
            {
                result = await _messaging.SendDonorMessageAsync(
                    UserId,
                    conversationId,
                    text);
            }
            else
            {
                throw new HubException("Only hospital admins and donors can send messages.");
            }

            if (!result.Success || result.Value == null)
                throw new HubException("Failed to save message.");

            // Get updated conversation snapshot for broadcast
            var convSnapshot = await GetConversationSnapshotAsync(conversationId);

            var payload = new NewMessagePayload
            {
                MessageDto      = result.Value,
                ConversationDto = convSnapshot
            };

            // Broadcast to all parties in the conversation group
            await Clients
                .Group(GroupName(conversationId))
                .SendAsync("message:new", payload);
        }

        // ── CLIENT METHOD: DeleteMessage ──────────────────────────
        public async Task DeleteMessage(string messageId, string conversationId)
        {
            if (UserType != nameof(UserTypes.HospitalAdmin))
                throw new HubException("Only hospital admins can delete messages.");

            await _messaging.DeleteMessageAsync(UserId, messageId);

            var payload = new MessageDeletedPayload
            {
                MessageId      = messageId,
                ConversationId = conversationId
            };

            await Clients
                .Group(GroupName(conversationId))
                .SendAsync("message:deleted", payload);
        }

        // ── CLIENT METHOD: Typing ─────────────────────────────────
        public async Task Typing(string conversationId, bool isTyping)
        {
            var payload = new TypingPayload
            {
                ConversationId = conversationId,
                SenderId       = UserId,
                SenderType     = UserType == nameof(UserTypes.HospitalAdmin)
                                    ? "Hospital" : "Donor",
                IsTyping       = isTyping
            };

            // Broadcast to everyone EXCEPT the sender
            await Clients
                .GroupExcept(GroupName(conversationId), Context.ConnectionId)
                .SendAsync("conversation:typing", payload);
        }

        // ── CLIENT METHOD: MarkRead ───────────────────────────────
        public async Task MarkRead(string conversationId)
        {
            string readByType;

            if (UserType == nameof(UserTypes.HospitalAdmin))
            {
                await _messaging.MarkConversationReadAsync(UserId, conversationId);
                readByType = "Hospital";
            }
            else if (UserType == nameof(UserTypes.Donor))
            {
                await _messaging.MarkConversationReadByDonorAsync(UserId, conversationId);
                readByType = "Donor";
            }
            else
            {
                throw new HubException("Unauthorized.");
            }

            var payload = new ReadPayload
            {
                ConversationId = conversationId,
                ReadByType     = readByType
            };

            await Clients
                .Group(GroupName(conversationId))
                .SendAsync("conversation:read", payload);
        }

        // ── Private: join hospital's conversation groups ──────────
        private async Task JoinHospitalGroupsAsync(string hospitalAdminUserId)
        {
            var admin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (admin == null) return;

            var conversationIds = await _context.Conversations
                .AsNoTracking()
                .Where(c => c.HospitalId == admin.HospitalId)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var cid in conversationIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(cid));
        }

        // ── Private: join donor's conversation groups ─────────────
        private async Task JoinDonorGroupsAsync(string donorUserId)
        {
            var donor = await _context.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.UserId == donorUserId && !d.IsDeleted);

            if (donor == null) return;

            var conversationIds = await _context.Conversations
                .AsNoTracking()
                .Where(c => c.DonorId == donor.Id)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var cid in conversationIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(cid));
        }

        // ── Private: broadcast donor online/offline to hospitals ──
        private async Task BroadcastDonorPresenceAsync(string donorUserId, bool online)
        {
            var donor = await _context.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.UserId == donorUserId && !d.IsDeleted);

            if (donor == null) return;

            // Find all hospitals that have conversations with this donor
            var hospitalConversations = await _context.Conversations
                .AsNoTracking()
                .Where(c => c.DonorId == donor.Id)
                .Select(c => new { c.Id, c.HospitalId })
                .ToListAsync();

            var payload = new PresencePayload
            {
                DonorId  = donor.Id,
                Online   = online,
                LastSeen = DateTime.UtcNow
            };

            // Broadcast to each conversation group
            foreach (var conv in hospitalConversations)
            {
                await Clients
                    .Group(GroupName(conv.Id))
                    .SendAsync("presence:update", payload);
            }
        }

        // ── Private: get conversation snapshot for broadcast ──────
        private async Task<ConversationDTO> GetConversationSnapshotAsync(
            string conversationId)
        {
            var conv = await _context.Conversations
                .AsNoTracking()
                .Where(c => c.Id == conversationId)
                .Select(c => new
                {
                    c.Id,
                    c.DonorId,
                    c.LastMessage,
                    c.LastMessageAt,
                    c.HospitalUnreadCount,
                    DonorFullName = _context.Donors
                        .Where(d => d.Id == c.DonorId)
                        .Select(d => d.User.FullName)
                        .FirstOrDefault(),
                    BloodTypeName = _context.Donors
                        .Where(d => d.Id == c.DonorId)
                        .Select(d => d.BloodType.TypeName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (conv == null) return new ConversationDTO();

            return new ConversationDTO
            {
                ConversationId = conv.Id,
                DonorId        = conv.DonorId,
                FullName       = conv.DonorFullName ?? "Unknown",
                BloodTypeName  = conv.BloodTypeName,
                Online         = _presence.IsOnline(conv.DonorId),
                LastMessage    = conv.LastMessage,
                LastMessageAt  = conv.LastMessageAt,
                UnreadCount    = conv.HospitalUnreadCount
            };
        }
    }
}
