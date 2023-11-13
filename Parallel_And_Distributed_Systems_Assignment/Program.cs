using System.Collections.Concurrent;
using System.Diagnostics;

namespace Parallel_And_Distributed_Systems_Assignment
{
    class Item
    {
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public int? Type { get; set; }
    }
    internal class Program
    {
        // Change the file path if you want to see the result of sorting
        // static string fileName = @"C:\\Users\\Andru\\source\\repos\\Parallel_And_Distributed_Systems\\Parallel_And_Distributed_Systems_Assignment\\";
        static object Locker = new object();
        static int countOne = 0;
        static int countTwo = 0;
        static int countThree = 0;
        static ConcurrentDictionary<int, int> typeNumOfItems = new ConcurrentDictionary<int, int>();
        static ConcurrentBag<string> itemOfTypeOne = new ConcurrentBag<string>();
        static ConcurrentBag<string> itemOfTypeSeven = new ConcurrentBag<string>();
        static ConcurrentBag<string> itemOfTypeTen = new ConcurrentBag<string>();
        static void Main(string[] args)
        {
            Random rnd = new Random();

            // ParallelBubbleSort

            Console.WriteLine("Parallel Bubble Sort");
            Console.WriteLine();

            List<int> numbers = new List<int>();

            for (int i = 0; i < 100000; i++)
            {
                numbers.Add(rnd.Next(0, 10000));
            }

            for (int i = 0; i < 4; i++)
            {
                int numOfThreads = i + 2;
                if (numOfThreads == 5)
                {
                    numOfThreads++;
                }
                ParallelBubbleSort(numbers, numOfThreads);
            }

            // ParallelFindBarcodes

            Console.WriteLine();
            Console.WriteLine("Parallel Find Barcodes");
            Console.WriteLine();

            List<Item> items = new List<Item>();

            int count = 1;

            for (int i = 0; i < 100000; i++)
            {
                if (count > 100)
                {
                    count = 1;
                }
                items.Add(new Item() { Barcode = (i + 1).ToString() + "b", Description = "Some description", Type = count });
                count++;
            }

            items = items.OrderBy(x => rnd.Next()).ToList();

            for (int i = 0; i < 4; i++)
            {
                int numOfThreads = i + 2;
                if (numOfThreads == 5)
                {
                    numOfThreads++;
                }
                FindBarcodes(items, numOfThreads);
            }
        }

        static void BubbleSort(List<int> numbers)
        {
            var n = numbers.Count;

            for (int i = 0; i < n; i++)
            {
                bool swapped = false;

                for (int j = 0; j < n - i - 1; j++)
                {
                    if (numbers[j] > numbers[j + 1])
                    {
                        int temp = numbers[j];
                        numbers[j] = numbers[j + 1];
                        numbers[j + 1] = temp;
                        swapped = true;
                    }
                }
                if (swapped == false)
                {
                    break;
                }
            }
        }

        static void ParallelBubbleSort(List<int> numbers, int numOfThreads)
        {
            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            List<List<int>> lists = new List<List<int>>();

            for (int i = 0; i < numOfThreads; i++)
            {
                lists.Add(new List<int>());
            }

            int count = numbers.Count / numOfThreads;
            for (int i = 0; i < lists.Count; i++)
            {
                lists[i].AddRange(numbers.Skip(i * count).Take(count));
            }

            List<Thread> activeThreads = new List<Thread>();

            foreach (var list in lists)
            {
                var thread = new Thread(() => BubbleSort(list));
                thread.Start();
                activeThreads.Add(thread);
            }

            foreach (var thread in activeThreads)
            {
                thread.Join();
            }

            List<int> result = new List<int>();

            for (int i = 0; i < lists.Count; i++)
            {
                result = result.Concat(lists[i]).ToList();
            }

            result = MergeSort(result);

            stopWatch.Stop();
            TimeSpan timeElapsed = stopWatch.Elapsed;
            string formatOfStopWatch = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds, timeElapsed.Milliseconds / 10);
            Console.WriteLine($"Time elapsed for {numOfThreads} threads: {formatOfStopWatch}");

            // Use the function below to see the result in a text file

            //using (StreamWriter writer = new StreamWriter(fileName + "numbers" + numOfThreads + ".txt"))
            //{
            //    foreach (var num in result)
            //    {
            //        writer.WriteLine(num);
            //    }
            //}
        }

        static void FindBarcodes(List<Item> items, int numOfThreads)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            stopWatch.Start();

            List<Thread> threads = new List<Thread>();

            List<List<Item>> lists = new List<List<Item>>();

            for (int i = 0; i < numOfThreads; i++)
            {
                lists.Add(new List<Item>());
            }

            int count = items.Count / numOfThreads;

            for (int i = 0; i < lists.Count; i++)
            {
                lists[i].AddRange(items.Skip(i * count).Take(count));
            }

            foreach (var list in lists)
            {
                var thread = new Thread(() => ParallelFindBarcodes(list));
                thread.Start();
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            foreach (var item in typeNumOfItems)
            {
                Console.WriteLine($"{item.Key} - {item.Value}");
            }

            stopWatch.Stop();
            TimeSpan timeElapsed = stopWatch.Elapsed;
            string formatOfStopWatch = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds, timeElapsed.Milliseconds / 10);
            Console.WriteLine($"Time elapsed to find the items for {numOfThreads} threads: {formatOfStopWatch}");

            Console.WriteLine();
            Console.WriteLine("Item of type one");
            Console.WriteLine();

            foreach (var barcode in itemOfTypeOne)
            {
                Console.WriteLine(barcode);
            }

            Console.WriteLine();
            Console.WriteLine("Item of type seven");
            Console.WriteLine();

            foreach (var barcode in itemOfTypeSeven)
            {
                Console.WriteLine(barcode);
            }

            Console.WriteLine();
            Console.WriteLine("Item of type ten");
            Console.WriteLine();

            foreach (var barcode in itemOfTypeTen)
            {
                Console.WriteLine(barcode);
            }
        }

        static List<int> MergeSort(List<int> numbers)
        {
            List<int> left = new List<int>();
            List<int> right = new List<int>();
            List<int> result = new List<int>(numbers.Count);

            if (numbers.Count <= 1)
            {
                return numbers;
            }

            int midPoint = numbers.Count / 2;

            left = new List<int>(midPoint);

            if (numbers.Count % 2 == 0)
            {
                right = new List<int>(midPoint);
            }
            else
            {
                right = new List<int>(midPoint + 1);
            }

            for (int i = 0; i < midPoint; i++)
            {
                left.Add(numbers[i]);
            }

            for (int i = midPoint; i < numbers.Count; i++)
            {
                right.Add(numbers[i]);
            }

            left = MergeSort(left);
            right = MergeSort(right);
            result = Merge(left, right);
            return result;
        }

        static List<int> Merge(List<int> left, List<int> right)
        {
            int resultLength = right.Count + left.Count;
            List<int> result = new List<int>(resultLength);

            int indexLeft = 0;
            int indexRight = 0;
            int indexResult = 0;

            while (indexLeft < left.Count || indexRight < right.Count)
            {
                if (indexLeft < left.Count && indexRight < right.Count)
                {
                    if (left[indexLeft] <= right[indexRight])
                    {
                        result.Add(left[indexLeft]);
                        indexLeft++;
                        indexResult++;
                    }
                    else
                    {
                        result.Add(right[indexRight]);
                        indexRight++;
                        indexResult++;
                    }
                }

                else if (indexLeft < left.Count)
                {
                    result.Add(left[indexLeft]);
                    indexLeft++;
                    indexResult++;
                }

                else if (indexRight < right.Count)
                {
                    result.Add(right[indexRight]);
                    indexRight++;
                    indexResult++;
                }
            }
            return result;
        }

        static void ParallelFindBarcodes(List<Item> items)
        {
            foreach (var item in items)
            {
                lock (Locker)
                {
                    if (countOne < 30)
                    {
                        if (item.Type == 1)
                        {
                            countOne++;
                            typeNumOfItems[1] = countOne;
                            itemOfTypeOne.Add(item.Barcode);
                        }
                    }
                }
                lock (Locker)
                {
                    if (countTwo < 15)
                    {
                        if (item.Type == 7)
                        {
                            countTwo++;
                            typeNumOfItems[7] = countTwo;
                            itemOfTypeSeven.Add(item.Barcode);
                        }
                    }
                }
                lock (Locker)
                {
                    if (countThree < 8)
                    {
                        if (item.Type == 10)
                        {
                            countThree++;
                            typeNumOfItems[10] = countThree;
                            itemOfTypeTen.Add(item.Barcode);
                        }
                    }
                }
                lock (Locker)
                {
                    if (countOne + countTwo + countThree == 53)
                    {
                        break;
                    }
                }
            }
        }
    }
}