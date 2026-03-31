namespace GitViewer.Api.RabbitMQ
{
    public class LogMessage<T>
    {
        public required string EventType { get; set; }
        public required string EntityType { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public Guid UserId { get; set; }
        public bool IsAnonymous { get; set; }
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; }
        public Guid? ShareLink { get; set; }
    }
}
