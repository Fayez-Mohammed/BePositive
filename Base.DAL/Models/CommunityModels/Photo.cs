using System;

namespace CommunityApp.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public int UserId { get; set; }
        public string Url { get; set; }
        public string Caption { get; set; }
        public DateTime UploadDate { get; set; }

        public virtual Post Post { get; set; }
        public virtual Comment Comment { get; set; }
      
    }
}
