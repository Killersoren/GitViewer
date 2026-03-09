using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GitViewer.DataAccess.Models
{
    public class Repository
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public long SizeInBytes { get; set; }
        public string Source { get; set; } // repository url
        public bool IsPublic { get; set; } = false;
        public Guid UserId { get; set; } // Foreign key for User
        public string? Status { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
