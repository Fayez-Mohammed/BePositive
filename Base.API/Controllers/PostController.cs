using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            var posts = await _postRepository.GetAllAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            var createdPost = await _postRepository.AddAsync(post);
            return CreatedAtAction(nameof(GetPost), new { id = createdPost.PostId }, createdPost);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, Post post)
        {
            if (id != post.PostId)
            {
                return BadRequest();
            }
            await _postRepository.UpdateAsync(post);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _postRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
