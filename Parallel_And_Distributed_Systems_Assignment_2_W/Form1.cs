using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parallel_And_Distributed_Systems_Assignment_2_W
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Paint += new PaintEventHandler(Form1_Paint);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // Graphics graphics = e.Graphics;

            Random rnd = new Random();

            List<Circle> coordinates = new List<Circle>();
            List<Worker> workers = new List<Worker>();

            int workerId = 1;

            for (int i = 0; i < 1000; i++)
            {
                int x = rnd.Next(0, 1001);
                int y = rnd.Next(0, 1001);

                coordinates.Add(new Circle(x, y, false));
            }

            for (int i = 0; i < 5; i++)
            {
                workers.Add(new Worker(10, 10));
            }

            foreach (var worker in workers)
            {
                worker.Draw(workerId++, coordinates);
            }
        }
    }

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

    internal class Worker : Form
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

        public async void Draw(int id, List<Circle> coordinates)
        {
            Circle coordinate = null;

            Pen pen = new Pen(Color.Black, 3);
            await Task.Run(async () =>
            {
                while (coordinates.Any(coord => coord.Painted == false))
                {
                    lock (Locker)
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
                    using (Graphics graphics = CreateGraphics())
                    {
                        graphics.DrawEllipse(pen, Math.Abs(coordinate.X - R), Math.Abs(coordinate.Y - R), R, R);
                        graphics.DrawString(id.ToString(), new Font("Arial", 16), new SolidBrush(Color.DarkOrange), Math.Abs(coordinate.X - R), Math.Abs(coordinate.Y - R));
                    }
                    CirclesDrawn.Add(coordinate);
                    // Console.WriteLine("Circle painted. Let me rest a bit");
                    // await Task.Delay(20);
                }
            });

            // Console.WriteLine($"Worker {id}: Drawing completed");

            // Console.WriteLine(CirclesDrawn.Count);
            // bitmap.Save("C:\\Users\\Andru\\source\\repos\\Parallel_And_Distributed_Systems\\Parallel_And_Distributed_Systems_Assignment_2\\Circle.jpg");
        }
    }
}
