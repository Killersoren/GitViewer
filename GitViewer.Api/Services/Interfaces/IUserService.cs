namespace GitViewer.Api.Services.Interfaces
{
    public interface IUserService
    {
        Guid GetRequiredUserId();
        Guid? TryGetOptionalUserId();
        //<Guid> GetUserIdAsync();
    }
}
