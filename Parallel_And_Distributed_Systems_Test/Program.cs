namespace Parallel_And_Distributed_Systems_Test
{
    internal class Program
    {
        static object Locker = new object();
        static bool isFound = false;
        static void Main(string[] args)
        {
            Random rnd = new Random();
            int numOfThreads = 5;

            List<int> numbers = new List<int>();

            for (int i = 0; i < 100000; i++)
            {
                numbers.Add(rnd.Next(0, 10000));
            }

            FindBigger(numbers, numOfThreads);
        }

        static void FindBigger(List<int> numbers, int numOfThreads)
        {
            List<List<int>> lists = new List<List<int>>();
            List<Thread> threads = new List<Thread>();
            int num = 0;

            int n = 100;

            for (int i = 0; i < numOfThreads; i++)
            {
                lists.Add(new List<int>());
            }

            int count = numbers.Count / numOfThreads;
            for (int i = 0; i < lists.Count; i++)
            {
                lists[i].AddRange(numbers.Skip(i * count).Take(count));
            }

            foreach (var list in lists)
            {
                var thread = new Thread(() => ParallelFindBigger(list, n, num));
                num++;
                thread.Start();
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        static void ParallelFindBigger(List<int> numbers, int n, int num)
        {
            foreach (var number in numbers)
            {
                lock (Locker)
                {
                    while (!isFound)
                    {
                        if (number > n)
                        {
                            Console.WriteLine($"Found number bigger than {n} which is {number} by {num}");
                            isFound = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}