using System;
using System.Collections.Generic;

namespace CommunityApp.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }

        public virtual Post Post { get; set; } 
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
    }
}
