using GitViewer.Api.RabbitMQ;
using GitViewer.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GitViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly GitViewerServiceContext _context;
        private readonly IMessageProducer _messageProducer;
        private readonly IGitRepoManager _gitManager;

        public UserController(GitViewerServiceContext context, IMessageProducer messageProducer)
        {
            _context = context;
            _messageProducer = messageProducer;
        }

        [Authorize(Roles = "user,admin")]
        [AllowAnonymous]
        [HttpGet("get-username")]
        public async Task<ActionResult<User>> GetUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user is null)
            {
                return NotFound("User does not exist");
            }

            Guid? ownerId = null;

            Claim? ownerIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);

            if (ownerIdClaim is not null)
            {
                ownerId = Guid.Parse(ownerIdClaim.Value);
            }

            if (!user.IsPublic)
            {
                if (ownerIdClaim is null)
                {
                    return Unauthorized("User user is not public");
                }

                userId = Guid.Parse(ownerIdClaim.Value);

                if (user.Id != userId)
                {
                    return Forbid();
                }
            }

            return Ok(user.UserName);
        }
    }
}
