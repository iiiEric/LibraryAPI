namespace LibraryAPI.Messaging.Services.Sender
{
    public interface IMessageBus
    {
        Task Publish<T>(T message, string queueName);
    }
}
