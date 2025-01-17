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
            //instantiating the message queue as a sender
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusSender sender = client.CreateSender(_queueName);

            //sending a message to the queue    
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message.Content);
            await sender.SendMessageAsync(serviceBusMessage);
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync()
        {
            var messages = new List<Message>();
            //instantiating the message queue as a receiver
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(_queueName);

            //getting the messages from the queue   
            var receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: 10);
            foreach (var receivedMessage in receivedMessages)
            {
                messages.Add(new Message { Content = receivedMessage.Body.ToString() });
                //deleting the message from the queue   
                await receiver.CompleteMessageAsync(receivedMessage);
            }

            return messages;
        }
    }
}
