using System;
using System.Collections.Generic;

namespace CommunityApp.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public int CommunityId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PostDate { get; set; }

        public virtual Community Community { get; set; }
 
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
    }
}
