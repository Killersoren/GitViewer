using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace GitViewer.Api.RabbitMQ
{
    public class RabbitMQProducer : IAsyncDisposable, IMessageProducer
    {
        private readonly ILogger<RabbitMQProducer> _logger;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;
        private bool _initialized;

        private readonly string _connectionString;
        private readonly IHostEnvironment _environment;

        public RabbitMQProducer(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQProducer> logger, IHostEnvironment environment)
        {
            _connectionString = settings.Value.ConnectionString;
            _logger = logger;
            _environment = environment;

            _factory = new ConnectionFactory();

            if (_environment.IsDevelopment())
            {
                _factory.HostName = "localhost";
            }
            else
            {
                _factory.Uri = new Uri(_connectionString);
            }
        }

        private async Task EnsureIntializedAsync()
        {
            if (_initialized)
                return;

            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(
                queue: "logs",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _initialized = true;
            _logger.LogInformation("RabbitMQ Producer initialized.");
        }

        public async Task SendMessage<T>(LogMessage<T> message)
        {
            await EnsureIntializedAsync();

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(exchange: "", routingKey: "logs", mandatory: false, body: body);

            _logger.LogInformation("Message sent to RabbitMQ.");
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }
            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
            _logger.LogInformation("RabbitMQ Producer disposed.");
        }
    }
}
