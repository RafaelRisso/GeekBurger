using Microsoft.Azure.ServiceBus;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBurguer.Queue.Sender
{
    class Program
    {
        const string QueueConnectionString = "Endpoint=sb://geekburguer.servicebus.windows.net/;SharedAccessKeyName=ProductPolicy;SharedAccessKey=5LwC65rXjh5/ayZCyA8+liQiMDwHiC23xsX0C6YUOkI=";
        const string QueuePath = "productchanged";
        static IQueueClient _queueClient;

        static void Main(string[] args)
        {
            SendMessagesAsync().GetAwaiter().GetResult();
            Console.WriteLine("messages were sent");
            Console.ReadLine();
        }

        private static async Task SendMessagesAsync()
        {
            _queueClient = new QueueClient(QueueConnectionString, QueuePath);
            var messages = "Hi,Hello,Hey,How are you,Be Welcome"
                .Split(',')
                .Select(msg =>
                {
                    Console.WriteLine($"Will send message: {msg}");
                    return new Message(Encoding.UTF8.GetBytes(msg));
                }).ToList();

            var sendTask = _queueClient.SendAsync(messages);
            await sendTask;
            CheckCommunicationExceptions(sendTask);

            var closeTask = _queueClient.CloseAsync();
            await closeTask;
            CheckCommunicationExceptions(closeTask);
        }

        public static bool CheckCommunicationExceptions(Task task)
        {
            if (task.Exception == null || task.Exception.InnerExceptions.Count == 0) return true;

            task.Exception.InnerExceptions.ToList()
                .ForEach(innerException =>
                {
                    Console.WriteLine($@"Error in SendAsync task: { innerException.Message}. Details: { innerException.StackTrace}");

                    if (innerException is ServiceBusCommunicationException)
                        Console.WriteLine("Connection Problem with Host");
                });

            return false;
        }
    }
}
