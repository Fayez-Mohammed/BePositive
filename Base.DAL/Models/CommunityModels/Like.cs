using System;

namespace CommunityApp.Models
{
    public class Like
    {
        public int LikeId { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public int UserId { get; set; }
        public DateTime LikeDate { get; set; }

        public virtual Post Post { get; set; }
        public virtual Comment Comment { get; set; }
        
    }
}
