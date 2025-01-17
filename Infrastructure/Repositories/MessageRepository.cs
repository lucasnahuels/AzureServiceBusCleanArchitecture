using Core.Entities;
using Core.Interfaces;
using Azure.Messaging.ServiceBus;

namespace Infrastructure.Repositories
{
    public class MessageRepository  
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public MessageRepository(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task AddMessageAsync(Message message)
        {
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusSender sender = client.CreateSender(_queueName);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message.Content);
            await sender.SendMessageAsync(serviceBusMessage);
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync()
        {
            var messages = new List<Message>();
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(_queueName);

            var receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: 10);
            foreach (var receivedMessage in receivedMessages)
            {
                messages.Add(new Message { Content = receivedMessage.Body.ToString() });
                await receiver.CompleteMessageAsync(receivedMessage);
            }

            return messages;
        }
    }
}
