using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Sender
{
    internal class Program
    {
        private static string _connectionString;
        private static string _queueName;
        private static async Task Main()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();
            _connectionString = config.GetSection("connectionString")?.Value;
            _queueName = config.GetSection("queueName")?.Value;

            // send a message to the queue
            await SendMessageAsync();
            // Console.WriteLine("Hello World!");
        }

        private static async Task SendMessageAsync()
        {
            // create a Service Bus client 
            await using (var client = new ServiceBusClient(_connectionString))
            {
                // create a sender for the queue 
                var sender = client.CreateSender(_queueName);

                // create a message that we can send
                var message = new ServiceBusMessage("Hello world!");

                // send the message
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Sent a single message to the queue: {_queueName}");
            }
        }
    }
}