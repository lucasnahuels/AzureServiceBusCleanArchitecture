using Core.Entities;
using Azure.Messaging.ServiceBus;
using System.Threading;

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


        //This approach is configured as a request-response pattern.
        //Disable the hosted service is needed if you want to use this approach
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

        //Alternative 2 to receive messages from the queue
        //This approach needs to be configured to run as a background service and not as a request-response pattern 
        /*To configure the MessagePumpApproach method to run as a background service when the application
        starts, you can create a hosted service in ASP.NET Core. This hosted service will start when the application 
        starts and run the MessagePumpApproach method.*/
        //This approach will run in a continous loop
        //This approach can improve performance by reducing the overhead of creating and disposing of the Service Bus client.
        public async Task MessagePumpApproach()
        {
            //instantiating the message queue as a receiver
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(_queueName);
            //while (!_cancellationToken.IsCancellationRequested)
            while (true)
            {
                // This worker thread runs every 30 seconds
                var messageList = await receiver.ReceiveMessagesAsync(10, TimeSpan.FromSeconds(30));

                // Process the messages
                foreach (var message in messageList)
                {
                    Console.WriteLine($"Received message: {message.Body}");
                    await receiver.CompleteMessageAsync(message);
                }
            }
        }

        //Alternative 3 to receive messages from the queue
        //See class ServiceBusHostedService in the Infrastructure/HostedServices folder
        //This approach is configured to run as a background service 
    }
}
