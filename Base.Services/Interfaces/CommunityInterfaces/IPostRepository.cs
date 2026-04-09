using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityApp.Models;

namespace CommunityApp.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post> GetByIdAsync(int id);
        Task<Post> AddAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(int id);
    }
}
