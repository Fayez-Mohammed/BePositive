using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Implementations
{
    public class PhotoRepository : IPhotoRepository
    {
        private static List<Photo> _photos = new List<Photo>();

        public async Task<Photo> GetByIdAsync(int id)
        {
            return await Task.FromResult(_photos.FirstOrDefault(p => p.PhotoId == id));
        }

        public async Task<IEnumerable<Photo>> GetByPostIdAsync(int postId)
        {
            return await Task.FromResult(_photos.Where(p => p.PostId == postId));
        }

        public async Task<Photo> AddAsync(Photo photo)
        {
            photo.PhotoId = _photos.Count > 0 ? _photos.Max(p => p.PhotoId) + 1 : 1;
            photo.UploadDate = DateTime.UtcNow;
            _photos.Add(photo);
            return await Task.FromResult(photo);
        }

        public async Task DeleteAsync(int id)
        {
            var photo = _photos.FirstOrDefault(p => p.PhotoId == id);
            if (photo != null)
            {
                _photos.Remove(photo);
            }
            await Task.CompletedTask;
        }
    }
}
