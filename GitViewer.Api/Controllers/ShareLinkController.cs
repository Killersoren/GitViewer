using GitViewer.Api.Services.Interfaces;
using GitViewer.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareLinkController : ControllerBase
    {
        private readonly IShareLinkService _shareLinkService;
        private readonly IUserService _userService;

        public ShareLinkController(IShareLinkService shareLinkService, IUserService userService)
        {
            _shareLinkService = shareLinkService;
            _userService = userService;
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("get-sharelinks")]
        public async Task<ActionResult<IAsyncEnumerable<ShareLink>>> GetShareLinksForUser()
        {
            var userId = _userService.GetRequiredUserId();

            var result = await _shareLinkService.GetShareLinksForUserAsync(userId);

            return Ok(result.Value);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("create-sharelink")]
        public async Task<ActionResult> CreateShareLinkUser(string name, int? expiryTimeInDays)
        {
            var userId = _userService.GetRequiredUserId();

            var result = await _shareLinkService.CreateShareLinkUserIdAsync(userId, name, expiryTimeInDays);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Share link created successfully");
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("delete-sharelink")]
        public async Task<ActionResult> DeleteShareLink(Guid ShareLinkId)
        {
            var userId = _userService.GetRequiredUserId();
            var result = await _shareLinkService.DeleteShareLinkAsync(userId, ShareLinkId);

            if (result.IsFailed)
            {
                return result.Errors.First().Message switch
                {
                    "Sharelink not found" => NotFound(result.Errors),
                    "Unauthorized" => Unauthorized(result.Errors),
                    _ => BadRequest(result.Errors)
                };
            }

            return Ok("Share link created successfully");
        }
    }
}
