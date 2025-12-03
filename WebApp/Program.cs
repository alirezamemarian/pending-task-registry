using Microsoft.Extensions.DependencyInjection;
using WebApp.Infrastructure;

namespace WebApp
{
    internal class Program
    {
        private const int RequestId = 1000;

        static async Task Main(string[] args)
        {
            // Setup dependency injection container

            var services = new ServiceCollection()
                .AddSingleton(typeof(IPendingTaskRegistry<,>), typeof(PendingTaskRegistry<,>))
                .BuildServiceProvider();

            // Resolve the pending task registry for int keys and string results

            var registry = services.GetRequiredService<IPendingTaskRegistry<int, string>>();

            // Start waiting for a response asynchronously

            var waitTask = registry.WaitForResponseAsync(RequestId);

            // Simulate an external callback that completes the pending task after a delay

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));

                if (registry.TrySetResult(RequestId, "Hello, World!"))
                {
                    Console.WriteLine("Callback completed successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to complete the callback.");
                }
            });

            // Await the result from the pending task

            var result = await waitTask;

            Console.WriteLine($"Received result: {result}");
        }
    }
}
