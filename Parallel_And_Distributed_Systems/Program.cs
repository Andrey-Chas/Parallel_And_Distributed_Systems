internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var cancellationTokenSource = new CancellationTokenSource(1500);

        var thread1 = new Thread((state) => DoWork(3, state));
        var thread2 = new Thread((state) => DoWork(5, state));
        var thread3 = new Thread((state) =>
        {
            Thread.Sleep(2000);
            (state as CancellationTokenSource).Cancel();
        });

        thread1.Start(cancellationTokenSource.Token);
        thread3.Start(cancellationTokenSource);
        thread2.Start(cancellationTokenSource.Token);

        static void DoWork(int indexCount, object? state)
        {
            var cancellationToken = (CancellationToken)state;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            for (int i = 0; i < indexCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine(
                        $"[{i}][{Thread.CurrentThread.ManagedThreadId}] cancelling");
                    return;
                }

                Console.WriteLine(
                    $"[{i}][{Thread.CurrentThread.ManagedThreadId}] I'm doing some work...");

                Thread.Sleep(1000);

                // Task.Delay(1000, cancellationToken).GetAwaiter().GetResult();

                Console.WriteLine(
                    $"[{i}][{Thread.CurrentThread.ManagedThreadId}] I'm done working");
            }
        }
    }
}

// DoWork(3);
// DoWork(5);

class ThreadParameters
{
    public string Name { get; }

    public CancellationToken cancellationToken { get; }
}