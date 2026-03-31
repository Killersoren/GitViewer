using GitViewer.Api.Services.Interfaces;
using System.Security.Claims;

namespace GitViewer.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetRequiredUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return Guid.Parse(userIdClaim.Value);
        }

        public Guid? TryGetOptionalUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim is null ? null : Guid.Parse(userIdClaim.Value);
        }


    }
}
