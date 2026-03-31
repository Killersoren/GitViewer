using System.ComponentModel.DataAnnotations.Schema;

namespace GitViewer.DataAccess.Models
{
    public class ShareLink
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryTime { get; set; }

        [ForeignKey("User")]
        public Guid? UserId { get; set; }
    }
}
