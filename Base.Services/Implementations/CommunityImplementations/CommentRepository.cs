using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private static List<Comment> _comments = new List<Comment>();

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
        {
            return await Task.FromResult(_comments.Where(c => c.PostId == postId));
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            return await Task.FromResult(_comments.FirstOrDefault(c => c.CommentId == id));
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            comment.CommentId = _comments.Count > 0 ? _comments.Max(c => c.CommentId) + 1 : 1;
            comment.CommentDate = DateTime.UtcNow;
            _comments.Add(comment);
            return await Task.FromResult(comment);
        }

        public async Task UpdateAsync(Comment comment)
        {
            var existingComment = _comments.FirstOrDefault(c => c.CommentId == comment.CommentId);
            if (existingComment != null)
            {
                existingComment.Content = comment.Content;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var comment = _comments.FirstOrDefault(c => c.CommentId == id);
            if (comment != null)
            {
                _comments.Remove(comment);
            }
            await Task.CompletedTask;
        }
    }
}
