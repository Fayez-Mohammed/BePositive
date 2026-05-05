// Base.DAL/Models/MessagingModels/ConversationModels.cs
//
// Add these two classes to your AppDbContext:
//   public DbSet<Conversation>    Conversations    { get; set; }
//   public DbSet<ChatMessage>     ChatMessages     { get; set; }
//
// Then run:
//   add-migration AddMessaging
//   update-database

using Base.DAL.Models.DonorModels;
using Base.DAL.Models.HospitalModels;

namespace Base.DAL.Models.MessagingModels
{
    /// <summary>
    /// One conversation = one hospital ↔ one donor thread.
    /// Unique constraint on (HospitalId, DonorId) — only one thread per pair.
    /// </summary>
    public class Conversation
    {
        public string Id           { get; set; } = Guid.NewGuid().ToString();
        public string HospitalId   { get; set; } = null!;
        public string DonorId      { get; set; } = null!;

        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt  { get; set; } = DateTime.UtcNow;

        // Last message cache — updated on every new message to avoid
        // expensive subqueries when loading the conversations list
        public string?  LastMessage     { get; set; }
        public DateTime? LastMessageAt  { get; set; }

        // Unread count from the hospital's perspective only
        // (donor unread is tracked on the Flutter side via FCM)
        public int HospitalUnreadCount  { get; set; } = 0;

        // Navigation
        public virtual Hospital Hospital { get; set; } = null!;
        public virtual Donor    Donor    { get; set; } = null!;
        public virtual ICollection<ChatMessage> Messages { get; set; }
            = new List<ChatMessage>();
    }

    /// <summary>
    /// Individual message in a conversation.
    /// SenderType tells us who sent it: "Hospital" or "Donor".
    /// Soft delete: IsDeleted = true hides from both sides.
    /// </summary>
    public class ChatMessage
    {
        public string Id             { get; set; } = Guid.NewGuid().ToString();
        public string ConversationId { get; set; } = null!;
        public string SenderId       { get; set; } = null!; // UserId
        public string SenderType     { get; set; } = null!; // "Hospital" | "Donor"

        public string?  Text         { get; set; }
        public string   Status       { get; set; } = "Sent"; // Sent | Delivered | Read

        public bool     IsDeleted    { get; set; } = false;
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt    { get; set; }

        // Navigation
        public virtual Conversation Conversation { get; set; } = null!;
    }
}
