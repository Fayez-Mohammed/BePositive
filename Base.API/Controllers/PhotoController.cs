using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoRepository _photoRepository;

        public PhotoController(IPhotoRepository photoRepository)
        {
            _photoRepository = photoRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Photo>> GetPhoto(int id)
        {
            var photo = await _photoRepository.GetByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }
            return Ok(photo);
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosByPost(int postId)
        {
            var photos = await _photoRepository.GetByPostIdAsync(postId);
            return Ok(photos);
        }

        [HttpPost]
        public async Task<ActionResult<Photo>> CreatePhoto(Photo photo)
        {
            var createdPhoto = await _photoRepository.AddAsync(photo);
            return CreatedAtAction(nameof(GetPhoto), new { id = createdPhoto.PhotoId }, createdPhoto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            await _photoRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
