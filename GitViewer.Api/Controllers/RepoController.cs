using GitViewer.Api.Dto;
using GitViewer.Api.Services;
using GitViewer.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GitViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepoController : ControllerBase
    {
        private readonly IRepositoryService _repositoryService;
        private readonly IGitFileService _gitFileService;
        private readonly IGitRepoManager _gitManager;

        public RepoController(IRepositoryService repositoryService, IGitFileService gitFileService, IGitRepoManager gitManager)
        {
            _repositoryService = repositoryService;
            _gitFileService = gitFileService;
            _gitManager = gitManager;
        }

        // GET
        [Authorize(Roles = "User,Admin")]
        [HttpGet("get-all-user-repos")]
        public async Task<ActionResult<IEnumerable<Repository>>> GetAllReposOfUser()
        {
            var userId = GetUserId();
            var result = await _repositoryService.GetUserReposAsync(userId);

            if (result.IsFailed)
            {
                return Unauthorized();
            }

            return Ok(result.Value);
        }

        [AllowAnonymous]
        [HttpGet("get-all-public-user-repos")]
        public async Task<ActionResult<IAsyncEnumerable<Repository>>> GetAllPublicUserRepos(Guid userId)
        {
            var requesterId = TryGetUserId();
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var result = await _repositoryService.GetPublicReposAsync(userId, requesterId, clientIp);

            if (result.IsFailed)
            {
                return result.Errors.First().Message switch
                {
                    "User not found" => NotFound("User does not exist"),
                    "Unauthorized" => Unauthorized("User is not public"),
                    "Forbidden" => Unauthorized("User is not public"),
                    _ => BadRequest(result.Errors.First().Message)
                };
            }

            return Ok(result.Value);
        }

        [AllowAnonymous]
        [HttpGet("get-single-repo")]
        public async Task<ActionResult<Repository>> GetRepo(Guid repoId)
        {
            var requesterId = TryGetUserId();
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var result = await _repositoryService.GetRepoAsync(repoId, requesterId, clientIp);

            if (result.IsFailed)
            {
                return result.Errors.First().Message switch
                {
                    "Repository not found" => NotFound("Repository does not exist"),
                    "Unauthorized" => Unauthorized(),
                    "Forbidden" => Forbid(),
                    _ => BadRequest(result.Errors.First().Message)
                };
            }

            return Ok(result.Value);
        }

        // POST
        [Authorize(Roles = "User,Admin")]
        [HttpPost("toggle-public")]
        public async Task<IActionResult> TogglePublic(Guid repoId, bool isPublic)
        {
            var userId = GetUserId();
            var result = await _repositoryService.TogglePublicAsync(repoId, userId, isPublic);

            if (result.IsFailed)
            {
                return result.Errors.First().Message switch
                {
                    "Repository not found" => NotFound("Repository not found"),
                    "Forbidden" => Forbid(),
                    _ => BadRequest(result.Errors.First().Message)
                };
            }

            return Ok();
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("add-repo")]
        public async Task<ActionResult<Repository>> AddRepo(RepositoryDto request)
        {
            var userId = GetUserId();
            var result = await _repositoryService.AddRepoAsync(request, userId);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.First().Message);
            }

            return Ok(result.Value);
        }

        // PATCH
        [Authorize(Roles = "User,Admin")]
        [HttpPatch("update-repo")]
        public async Task<ActionResult<Repository>> UpdateRepo([FromQuery] Guid repoId, [FromBody] RepositoryDto request)
        {
            var userId = GetUserId();
            var result = await _repositoryService.UpdateRepoAsync(repoId, userId, request);

            if (result.IsFailed)
            {
                return result.Errors.First().Message switch
                {
                    "Repository not found" => NotFound("Repository not found"),
                    "Forbidden" => Forbid(),
                    _ => BadRequest(result.Errors.First().Message)
                };
            }

            return Ok();
        }

        // DELETE
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("delete-repo")]
        public async Task<ActionResult> DeleteRepo(Guid repoId)
        {
            var userId = GetUserId();
            var result = await _repositoryService.DeleteRepoAsync(repoId, userId);

            if (result.IsFailed)
            {
                return result.Errors.First().Message switch
                {
                    "Repository not found" => NotFound("Repository not found"),
                    "Forbidden" => Forbid(),
                    _ => BadRequest(result.Errors.First().Message)
                };
            }

            return Ok();
        }

        // Deploy keys & cloning
        [Authorize(Roles = "User,Admin")]
        [HttpPost("generate-deploy-key")]
        public async Task<ActionResult> GenerateDeployKey(Guid repoId)
        {
            var currentUserId = GetUserId();

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Check repo access
            var repoResult = await _repositoryService.GetRepoAsync(repoId, currentUserId, clientIp);
            if (repoResult.IsFailed)
            {
                return Forbid();
            }

            var (privateKey, publicKey) = _gitManager.GenerateDeployKey(repoResult.Value.UserId, repoResult.Value.Id);

            return Ok(new { repoId, publicKey });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("clone-repo")]
        public async Task<IActionResult> CloneRepo([FromQuery] Guid repoId, [FromQuery] bool useSsh = true)
        {
            var currentUserId = GetUserId();
            var result = await _repositoryService.CloneRepoAsync(repoId, currentUserId, useSsh);

            if (result.IsFailed)
            {
                return result.Errors.First().Message switch
                {
                    "Repository not found" => NotFound("Repository not found"),
                    "Forbidden" => Forbid(),
                    _ => BadRequest(result.Errors.First().Message)
                };
            }

            return Ok("Cloned successfully");
        }

        // File operations
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree(Guid repoId, [FromQuery] string? path)
        {
            try
            {
                var result = await _gitFileService.GetTreeAsync(repoId, path);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("file")]
        public async Task<IActionResult> GetFile(Guid repoId, [FromQuery] string path)
        {
            try
            {
                var content = await _gitFileService.GetFileContentAsync(repoId, path);
                return Ok(new { content });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadRepo(Guid repoId)
        {
            try
            {
                var (stream, fileName) = await _gitFileService.DownloadAsync(repoId);
                return File(stream, "application/zip", fileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return Guid.Parse(userIdClaim.Value);
        }

        private Guid? TryGetUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim is null ? null : Guid.Parse(userIdClaim.Value);
        }
    }
}
