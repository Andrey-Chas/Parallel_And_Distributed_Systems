class Counter
{
    public int Count { get; protected set; }

    public void IncrementCounter()
    {
        Count++;
    }
}

internal class Program
{
    static Counter Counter = new Counter();
    static object Locker = new object();
    static SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    static ManualResetEvent ManualEvent = new ManualResetEvent(false);

    private static void Main(string[] args)
    {
        var threads = new List<Thread>();
        Console.WriteLine($"The current count is: {Counter.Count}");

        for (int i = 0; i < 3; i++)
        {
            var thread = new Thread((state) => DoWork(state?.ToString()));
            threads.Add(thread);

            thread.Start($"Thread No. {i + 1}");
        }

        //new Thread(() =>
        //{
        //    for (int i = 0; i < 10; i++)
        //    {
        //        Console.WriteLine("Some work to be done before the threads continue...");
        //        Thread.Sleep(1000);
        //    }

        //    ManualEvent.Set();

        //}).Start();

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Console.ReadKey();
    }

    static void DoWork(string? name)
    {
        lock (Locker)
        {
            for (int i = 0; i < 5; i++)
            {
                Counter.IncrementCounter();
            }
            Console.WriteLine($"The current count is: {Counter.Count}");
        }
    }

    static void DoWorkLocker(string? name)
    {
        lock (Locker)
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Doing work [{i + 1}] from {name}");
                Thread.Sleep(1000);
            }
        }
    }

    static void DoWorkSemaphore(string? name)
    {
        Semaphore.Wait();

        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"Doing work [{i + 1}] from {name}" + $"Free slots for threads: {Semaphore.CurrentCount}");
            Thread.Sleep(1000);
        }

        Semaphore.Release();
    }

    static void DoWorkManualResetEvent(string? name)
    {
        ManualEvent.WaitOne();

        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"Doing work [{i + 1}] from {name}" + $"Free slots for threads: {Semaphore.CurrentCount}");
            Thread.Sleep(1000);
        }

        ManualEvent.Reset();
    }
}