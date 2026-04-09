using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommunityApp.Interfaces;
using CommunityApp.Models;

namespace CommunityApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;

        public CommentController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByPost(int postId)
        {
            var comments = await _commentRepository.GetByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment(Comment comment)
        {
            var createdComment = await _commentRepository.AddAsync(comment);
            return CreatedAtAction(nameof(GetComment), new { id = createdComment.CommentId }, createdComment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, Comment comment)
        {
            if (id != comment.CommentId)
            {
                return BadRequest();
            }
            await _commentRepository.UpdateAsync(comment);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _commentRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
