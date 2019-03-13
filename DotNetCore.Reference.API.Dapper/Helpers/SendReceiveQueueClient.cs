namespace DotNetCore.Reference.API.Dapper.Helpers
{
    using DotNetCore.Reference.API.Dapper.Models;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class SendReceiveQueueClient
    {
        const string ServiceBusConnectionString = "Endpoint=;SharedAccessKeyName=;SharedAccessKey==";
        const string QueueName = "****";
        private IQueueClient queueClient;

        public SendReceiveQueueClient()
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            //Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        // Use this Handler to look at the exceptions received on the MessagePump
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            //Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            //var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            //Console.WriteLine("Exception context for troubleshooting:");
            //Console.WriteLine($"- Endpoint: {context.Endpoint}");
            //Console.WriteLine($"- Entity Path: {context.EntityPath}");
            //Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public async Task SendMessagesAsync(Agent item)
        {
            try
            {
                dynamic data = new { name = item.Name, location = item.Location, active = item.Active };
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
                {
                    ContentType = "application/json",
                    Label = "Agent",
                    MessageId = "123456",
                    TimeToLive = TimeSpan.FromMinutes(5)
                };

                await queueClient.SendAsync(message);
            }
            catch (Exception)
            {
                //Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        public async Task CloseAsync()
        {
            await queueClient.CloseAsync();
        }
    }
}