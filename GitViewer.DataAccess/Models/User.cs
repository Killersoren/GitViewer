using System.ComponentModel.DataAnnotations;

namespace GitViewer.DataAccess.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Email { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Unverified";
        public bool IsPublic { get; set; } = true; // Dertimes if a user can be shared with others (Shows all public repositories)


        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public List<Repository> Repositories { get; set; } = new();
    }
}
