using Azure.Messaging.ServiceBus;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.HostedServices
{
    public class ServiceBusHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<ServiceBusHostedService> _logger;
        private readonly IConfiguration _configuration;
        private ServiceBusClient _serviceBusClient;

        public ServiceBusHostedService(ILogger<ServiceBusHostedService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //instantiating the message queue as a receiver
            _serviceBusClient = new ServiceBusClient(_configuration["ServiceBus:ConnectionString"]);

            // Create a processor for a queue
            var processor = _serviceBusClient.CreateProcessor(_configuration["ServiceBus:QueueName"], new ServiceBusProcessorOptions());

            // Register the message handler
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            // Start the processor
            await processor.StartProcessingAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop the processor
            await _serviceBusClient.DisposeAsync();
        }

        // Message handler
        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            // Process the message
            var message = args.Message;
            _logger.LogInformation($"Received message: {message.Body}");

            // Complete the message
            await args.CompleteMessageAsync(args.Message);
        }

        // Error handler
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            // Handle the error
            _logger.LogError($"Error occurred: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Dispose the Service Bus client
            _serviceBusClient.DisposeAsync();
        }
    }
}
