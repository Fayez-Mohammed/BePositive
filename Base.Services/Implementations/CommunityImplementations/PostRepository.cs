using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Implementations
{
    public class PostRepository : IPostRepository
    {
        // Mocking database with a static list for demonstration
        private static List<Post> _posts = new List<Post>();

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await Task.FromResult(_posts);
        }

        public async Task<Post> GetByIdAsync(int id)
        {
            return await Task.FromResult(_posts.FirstOrDefault(p => p.PostId == id));
        }

        public async Task<Post> AddAsync(Post post)
        {
            post.PostId = _posts.Count > 0 ? _posts.Max(p => p.PostId) + 1 : 1;
            post.PostDate = DateTime.UtcNow;
            _posts.Add(post);
            return await Task.FromResult(post);
        }

        public async Task UpdateAsync(Post post)
        {
            var existingPost = _posts.FirstOrDefault(p => p.PostId == post.PostId);
            if (existingPost != null)
            {
                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var post = _posts.FirstOrDefault(p => p.PostId == id);
            if (post != null)
            {
                _posts.Remove(post);
            }
            await Task.CompletedTask;
        }
    }
}
