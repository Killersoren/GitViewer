namespace GitViewer.Api.RabbitMQ
{
    public interface IMessageProducer
    {
        Task SendMessage<T>(LogMessage<T> message);
    }
}
