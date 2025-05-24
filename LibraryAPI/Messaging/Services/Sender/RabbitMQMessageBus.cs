using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LibraryAPI.Messaging.Services.Sender
{
    public class RabbitMQMessageBus : IMessageBus
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQMessageBus(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"]!,
                UserName = configuration["RabbitMQ:UserName"]!,
                Password = configuration["RabbitMQ:Password"]!
            };
        }

        public async Task Publish<T>(T message, string queueName)
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
        }
    }
}
