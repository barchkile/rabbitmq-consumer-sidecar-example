using System;
using System.Threading.Tasks;

namespace RabbitMQ.Consumer;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("Main starting...");
        var microService = new ConsumerMicroService();
        await microService.RunAsync();
        Console.WriteLine("Main exiting");
    }
}