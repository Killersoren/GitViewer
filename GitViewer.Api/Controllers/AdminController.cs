using GitViewer.Api.Dto;
using GitViewer.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GitViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(GitViewerServiceContext context) : ControllerBase
    {
        //GET
        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-repos")]
        public async Task<ActionResult<IAsyncEnumerable<Repository>>> GetAllRepos()
        {
            var repos = await context.Repository
                .ToListAsync();

            return Ok(repos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-single-repo-admin")]
        public async Task<ActionResult<Repository>> GetRepo(Guid repoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                return Unauthorized();
            }

            var repo = await context.Repository.FindAsync(repoId);
            if (repo is null)
            {
                return BadRequest("User does not exist");
            }
            return Ok(repo);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-users")]
        public async Task<ActionResult<IAsyncEnumerable<User>>> GetAllUsers()
        {
            var users = await context.Users
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-user")]
        public async Task<ActionResult<IAsyncEnumerable<User>>> GetUser(Guid userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                return Unauthorized();
            }

            var user = await context.Users.FindAsync(userId);

            if (user is null)
            {
                return BadRequest("user does not exist");
            }
            return Ok(user);
        }

        //POST

        //PATCH
        [Authorize(Roles = "Admin")]
        [HttpPatch("update-role/{userId}")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UserRoleDto request)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is null)
            {
                return BadRequest("user does not exist");
            }

            user.Role = request.Role;
            await context.SaveChangesAsync();

            return Ok("Role updated successfully");
        }


        //DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is null)
            {
                return BadRequest("user does not exist");
            }

            context.Users.Remove(user);

            await context.SaveChangesAsync();

            return Ok($"User {user.UserName} successfully deleted");
        }
    }
}
