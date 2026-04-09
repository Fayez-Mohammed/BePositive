using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly ILikeRepository _likeRepository;

        public LikeController(ILikeRepository likeRepository)
        {
            _likeRepository = likeRepository;
        }

        [HttpGet("post/{postId}/count")]
        public async Task<ActionResult<int>> GetPostLikeCount(int postId)
        {
            return Ok(await _likeRepository.GetCountForPostAsync(postId));
        }

        [HttpGet("comment/{commentId}/count")]
        public async Task<ActionResult<int>> GetCommentLikeCount(int commentId)
        {
            return Ok(await _likeRepository.GetCountForCommentAsync(commentId));
        }

        [HttpPost]
        public async Task<ActionResult<Like>> CreateLike(Like like)
        {
            var createdLike = await _likeRepository.AddAsync(like);
            return Ok(createdLike);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLike(int id)
        {
            await _likeRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
