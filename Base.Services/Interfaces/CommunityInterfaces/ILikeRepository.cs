using System.Threading.Tasks;
using CommunityApp.Models;

namespace CommunityApp.Interfaces
{
    public interface ILikeRepository
    {
        Task<Like> GetByIdAsync(int id);
        Task<Like> AddAsync(Like like);
        Task DeleteAsync(int id);
        Task<int> GetCountForPostAsync(int postId);
        Task<int> GetCountForCommentAsync(int commentId);
    }
}
