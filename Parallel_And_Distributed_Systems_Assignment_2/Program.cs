using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace Parallel_And_Distributed_Systems_Assignment_2
{
    internal class Circle
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int WorkerId { get; set; }
        public bool Painted { get; set; }

        public Circle(int id, int x, int y, bool painted)
        {
            Id = id;
            X = x;
            Y = y;
            Painted = painted;
        }
    }

    internal class Worker
    {
        private readonly static object Locker = new object();
        public int N { get; set; }
        public int M { get; set; }
        public int R { get; set; }
        public List<Circle> CirclesDrawn = new List<Circle>();

        public Worker(int n, int m)
        {
            N = n;
            M = m;
            R = (int)((n * m) / 2 * 3.14);
        }

        public async Task Draw(int id, Bitmap bitmap, Graphics graphics, List<Circle> coordinates, List<int> numberOfCirclesDrawn, int numOfWorkers, string fileName, List<Circle> circlesDrawn)
        {
            Pen pen = new Pen(Color.Black, 3);
            Circle coordinate = null;

            graphics.Clear(Color.White);
            await Task.Run(async () =>
            {
                while (coordinates.Any(coord => coord.Painted == false))
                {
                    lock (Locker)
                    {
                        coordinate = coordinates.FirstOrDefault(coord => coord.Painted == false);

                        if (coordinate != null)
                        {
                            coordinate.Painted = true;
                            coordinate.WorkerId = id;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //if (!coordinates.Any(coord => coord.Painted == false))
                    //{
                    //    Console.WriteLine("Completed");
                    //    break;
                    //}

                    // [taskId]

                    await Task.Delay(20);
                    graphics.DrawEllipse(pen, Math.Abs(coordinate.X - R), Math.Abs(coordinate.Y - R), R, R);
                    graphics.DrawString(id.ToString(), new Font("Arial", 16), new SolidBrush(Color.DarkOrange), Math.Abs(coordinate.X - R), Math.Abs(coordinate.Y - R));
                    CirclesDrawn.Add(coordinate);
                }
            });

            Console.WriteLine($"{CirclesDrawn.Count} circles was drawn by worker number {id}");

            numberOfCirclesDrawn.Add(CirclesDrawn.Count);
            lock (Locker)
            {
                foreach (var coord in CirclesDrawn)
                {
                    circlesDrawn.Add(coord);
                }
            }

            // Change the path to see the representation of the circles

            bitmap.Save("C:\\Users\\Andru\\source\\repos\\Parallel_And_Distributed_Systems\\Parallel_And_Distributed_Systems_Assignment_2\\Circle" + numOfWorkers + ".jpg");
        }
    }

    internal class Program
    {
        // Change the path to see the detailed information about circles and workers

        static string fileName = @"C:\\Users\\Andru\\source\\repos\\Parallel_And_Distributed_Systems\\Parallel_And_Distributed_Systems_Assignment_2\\";
        static void Main(string[] args)
        {
            Bitmap bitmap = new Bitmap(1000, 1000, PixelFormat.Format32bppPArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            Random rnd = new Random();

            List<Circle> coordinates = new List<Circle>();
            List<Circle> circlesDrawn = new List<Circle>();
            List<Worker> workers = new List<Worker>();
            List<int> numberOfCirclesDrawn = new List<int>();
            int workerId = 1;
            // int x = 0;
            // int y = 0;

            for (int i = 0; i < 1000; i++)
            {
                int x = rnd.Next(0, 1001);
                int y = rnd.Next(0, 1001);

                //x += 30;
                //if (x >= 1000)
                //{
                //    x = 0;
                //    y += 35;
                //}

                coordinates.Add(new Circle(i + 1, x, y, false));
            }

            DrawCircles(5, workers, workerId, bitmap, graphics, coordinates, numberOfCirclesDrawn, circlesDrawn);
            workers.Clear();
            workerId = 1;
            numberOfCirclesDrawn.Clear();
            circlesDrawn.Clear();
            foreach (var coordinate in coordinates)
            {
                coordinate.Painted = false;
            }
            Thread.Sleep(1000);

            DrawCircles(20, workers, workerId, bitmap, graphics, coordinates, numberOfCirclesDrawn, circlesDrawn);
            workers.Clear();
            workerId = 1;
            numberOfCirclesDrawn.Clear();
            circlesDrawn.Clear();
            foreach (var coordinate in coordinates)
            {
                coordinate.Painted = false;
            }
            Thread.Sleep(1000);

            DrawCircles(100, workers, workerId, bitmap, graphics, coordinates, numberOfCirclesDrawn, circlesDrawn);

            Console.ReadKey();
        }

        public static void DrawCircles(int numOfWorkers, List<Worker> workers, int workerId, Bitmap bitmap, Graphics graphics, List<Circle> coordinates, List<int> numberOfCirclesDrawn, List<Circle> circlesDrawn)
        {
            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            for (int i = 0; i < numOfWorkers; i++)
            {
                workers.Add(new Worker(20, 1));
            }

            List<Task> tasks1 = new List<Task>();

            foreach (var worker in workers)
            {
                tasks1.Add(worker.Draw(workerId++, bitmap, graphics, coordinates, numberOfCirclesDrawn, numOfWorkers, fileName, circlesDrawn));
            }

            Task.WhenAll(tasks1).Wait();

            using (StreamWriter writer = new StreamWriter(fileName + "separateCircles" + numOfWorkers + ".txt"))
            {
                foreach (var circle in circlesDrawn)
                {
                    writer.WriteLine($"The circle number {circle.Id} at coordinates {circle.X} and {circle.Y} was drawn by worker number {circle.WorkerId}");
                }
            }

            Console.WriteLine("The number of circles drawn is " + numberOfCirclesDrawn.Sum());

            stopWatch.Stop();
            TimeSpan timeElapsed = stopWatch.Elapsed;
            string formatOfStopWatch = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds, timeElapsed.Milliseconds / 10);
            Console.WriteLine($"Time elapsed for {numOfWorkers} workers is {formatOfStopWatch}");
            Console.WriteLine();
        }
    }
}