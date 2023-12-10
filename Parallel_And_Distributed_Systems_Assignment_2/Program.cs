using System.Drawing;
using System.Drawing.Imaging;

namespace Parallel_And_Distributed_Systems_Assignment_2
{
    internal class Circle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Painted { get; set; }

        public Circle(int x, int y, bool painted)
        {
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

        public async void Draw(int id, Bitmap bitmap, Graphics graphics, List<Circle> coordinates)
        {
            Pen pen = new Pen(Color.Black, 3);
            Circle coordinate = null;

            graphics.Clear(Color.White);
            await Task.Run(async () =>
            {
                while (coordinates.Any(coord => coord.Painted == false))
                {
                    lock(Locker)
                    {
                        if (coordinates.Any(coordin => coordin.Painted == false))
                        {
                            coordinate = coordinates.First(coord => coord.Painted == false);
                            coordinate.Painted = true;
                        }
                        else
                        {
                            break;
                        }
                        // [taskId]
                    }
                    await Task.Delay(20);
                    graphics.DrawEllipse(pen, Math.Abs(coordinate.X - R), Math.Abs(coordinate.Y - R), R, R);
                    graphics.DrawString(id.ToString(), new Font("Arial", 16), new SolidBrush(Color.DarkOrange), Math.Abs(coordinate.X - R), Math.Abs(coordinate.Y - R));
                    CirclesDrawn.Add(coordinate);
                    // Console.WriteLine("Circle painted. Let me rest a bit");
                    // await Task.Delay(20);
                }
            });

            // Console.WriteLine($"Worker {id}: Drawing completed");

            Console.WriteLine(CirclesDrawn.Count);
            bitmap.Save("C:\\Users\\Andru\\source\\repos\\Parallel_And_Distributed_Systems\\Parallel_And_Distributed_Systems_Assignment_2\\Circle.jpg");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Bitmap bitmap = new Bitmap(1000, 1000, PixelFormat.Format32bppPArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            Random rnd = new Random();

            List<Circle> coordinates = new List<Circle>();
            List<Worker> workers = new List<Worker>();
            List<List<Circle>> lists = new List<List<Circle>>();
            // int countLists = 0;
            int workerId = 1;

            for (int i = 0; i < 1000; i++)
            {
                int x = rnd.Next(0, 1001);
                int y = rnd.Next(0, 1001);

                coordinates.Add(new Circle(x, y, false));
            }

            for (int i = 0; i < 20; i++)
            {
                workers.Add(new Worker(10, 10));
            }

            for (int i = 0; i < workers.Count; i++)
            {
                lists.Add(new List<Circle>());
            }

            int count = coordinates.Count / workers.Count;
            if (coordinates.Count % workers.Count != 0)
            {
                count++;
            }

            for (int i = 0; i < lists.Count; i++)
            {
                lists[i].AddRange(coordinates.Skip(i * count).Take(count));
            }

            // Worker worker = new Worker(10, 10);

            List<Thread> threads = new List<Thread>();

            List<Task<int>> numbers = new List<Task<int>>();

            List<Task<int>> tasks = new List<Task<int>>();

            foreach (var worker in workers)
            {
                worker.Draw(workerId++ , bitmap, graphics, coordinates);
            }

            Console.ReadKey();
        }
    }
}