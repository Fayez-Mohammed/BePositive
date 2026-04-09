using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityApp.Models;

namespace CommunityApp.Interfaces
{
    public interface IPhotoRepository
    {
        Task<Photo> GetByIdAsync(int id);
        Task<IEnumerable<Photo>> GetByPostIdAsync(int postId);
        Task<Photo> AddAsync(Photo photo);
        Task DeleteAsync(int id);
    }
}
