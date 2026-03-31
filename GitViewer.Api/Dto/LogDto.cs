namespace GitViewer.Api.Dto
{
    public class LogDto
    {
        // Event information
        public string EventType { get; set; } = string.Empty; // e.g. "RepositoryCreated", "UserRegistered"
        public string EntityType { get; set; } = string.Empty; // e.g. "Repository", "User"
        public string EntityName { get; set; } = string.Empty; // e.g. repo name or username
        public Guid? EntityId { get; set; }

        // User information
        public Guid? UserId { get; set; }

        public bool IsAnonymous => UserId == null;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Other stuff
        public string? Details { get; set; } // optional stuff
        public Guid? ShareLink { get; set; } // Used if a share link was involved in the event
    }
}
