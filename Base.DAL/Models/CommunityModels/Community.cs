using System;
using System.Collections.Generic;

namespace CommunityApp.Models
{
    public class Community
    {
        public int CommunityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public int OwnerId { get; set; } 

        public virtual ICollection<Post> Posts { get; set; }
    }
}
