using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Receiver
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
          
            // receive message from the queue
            await ReceiveMessagesAsync();
        }

        private static async Task ReceiveMessagesAsync()
        {
            await using var client = new ServiceBusClient(_connectionString);
            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();

            Console.WriteLine("Wait for a minute and then press any key to end the processing");
            Console.ReadKey();

            // stop processing 
            Console.WriteLine("\nStopping the receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");
        }

        // handle received messages
        private static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            // complete the message. messages is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}