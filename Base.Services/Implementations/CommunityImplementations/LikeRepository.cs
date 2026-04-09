using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Implementations
{
    public class LikeRepository : ILikeRepository
    {
        private static List<Like> _likes = new List<Like>();

        public async Task<Like> GetByIdAsync(int id)
        {
            return await Task.FromResult(_likes.FirstOrDefault(l => l.LikeId == id));
        }

        public async Task<Like> AddAsync(Like like)
        {
            like.LikeId = _likes.Count > 0 ? _likes.Max(l => l.LikeId) + 1 : 1;
            like.LikeDate = DateTime.UtcNow;
            _likes.Add(like);
            return await Task.FromResult(like);
        }

        public async Task DeleteAsync(int id)
        {
            var like = _likes.FirstOrDefault(l => l.LikeId == id);
            if (like != null)
            {
                _likes.Remove(like);
            }
            await Task.CompletedTask;
        }

        public async Task<int> GetCountForPostAsync(int postId)
        {
            return await Task.FromResult(_likes.Count(l => l.PostId == postId));
        }

        public async Task<int> GetCountForCommentAsync(int commentId)
        {
            return await Task.FromResult(_likes.Count(l => l.CommentId == commentId));
        }
    }
}
