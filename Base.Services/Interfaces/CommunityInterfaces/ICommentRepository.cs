using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityApp.Models;

namespace CommunityApp.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
        Task<Comment> GetByIdAsync(int id);
        Task<Comment> AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
    }
}
