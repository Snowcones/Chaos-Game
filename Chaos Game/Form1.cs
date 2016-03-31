using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        const int width = 1440;
        const int height = 900;

        Random rand = new Random();
        Bitmap bmp = new Bitmap(1440, 900);

        double[] window = { -2, 2, -2, 2 };

        int[,] visitedPixels = new int[width, height];

        //Takes a list of functions, with the same form as runGame's functions, and a list of cumulative probabilities for the functions and iterates with them
        void runProbGame(List<double[]> functions, int iter)
        {
            double minX = window[0];
            double maxX = window[1];
            double minY = window[2];
            double maxY = window[3];

            Coordinates pos = new Coordinates(0, 0);
            Coordinates temp = new Coordinates(0, 0);
            
            int x = 0, y = 0;

            for (int i = 0; i < iter; i++)
            {
                int p = rand.Next(functions.Count);   //Choose random function

                //Perform affine transform
                temp.x = pos.x * functions.ElementAt(p)[0] + pos.y * functions.ElementAt(p)[1] + functions.ElementAt(p)[4];
                temp.y = pos.x * functions.ElementAt(p)[2] + pos.y * functions.ElementAt(p)[3] + functions.ElementAt(p)[5];
                
                pos.x = temp.x;     //Store transformation
                pos.y = temp.y;

                if (pos.x <= maxX && pos.x >= minX && pos.y <= maxY && pos.y >= minY)
                {
                    double normalizedX = (pos.x - minX) / (maxX - minX);
                    x = (int)(normalizedX * width);

                    double normalizedY = (pos.y - minY) / (maxY - minY);
                    y = (int)(height - normalizedY * height);
                }

                visitedPixels[x, y]++;
            }
        }

        void randomLinearFractal(int functionNumber, int iterations) //Returns number of distinct pixels visited on the screen
        {
            List <double[]> functions = new List<double[]>();

            for (int f = 0; f < functionNumber; f++)
            {
                functions.Add(new double[6]);

                for (int c = 0; c < 6; c++)
                {
                    functions.ElementAt(f)[c] = 2 * rand.NextDouble() - 1.0;
                }
            }

            runProbGame(functions, iterations);
        }

        void renderBinaryBitmap(int[,] dataSource, Bitmap drawingMap, Color offColor, Color onColor)    //Stores data in dataSource into the drawingMap Bitmap
        {
            for (int x = 0; x < dataSource.GetLength(0); x++)
            {
                for (int y = 0; y < dataSource.GetLength(1); y++)
                {
                    if (dataSource[x, y] == 0)
                    {
                        drawingMap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        drawingMap.SetPixel(x, y, Color.White);
                    }
                }
            }
        }

        void renderLoggedBitmap(int[,] dataSource, Bitmap drawingMap, Color offColor, Color onColor)
        {
            int[] flatData = dataSource.Cast<int>().ToArray();

            double[,] loggedData = new double[dataSource.GetLength(0), dataSource.GetLength(1)];

            for (int x = 0; x < dataSource.GetLength(0); x++)
            {
                for (int y = 0; y < dataSource.GetLength(1); y++)
                {
                    if (dataSource[x, y] != 0)
                    {
                        loggedData[x, y] = Math.Log(Convert.ToDouble(dataSource[x, y]), 2);
                    }
                    else
                    {
                        loggedData[x, y] = 0;
                    }
                }
            }

            double max = loggedData.Cast<double>().Max();

            double[] flat = loggedData.Cast<double>().ToArray();

            for (int x = 0; x < loggedData.GetLength(0); x++)
            {
                for (int y = 0; y < loggedData.GetLength(1); y++)
                {
                    int greyscale = (int)(255 * loggedData[x, y] / max);

                    if (loggedData[x, y] == 0)
                    {
                        drawingMap.SetPixel(x, y, Color.FromArgb(greyscale, greyscale, greyscale));
                    }
                    else
                    {
                        drawingMap.SetPixel(x, y, Color.FromArgb(greyscale, greyscale, greyscale));
                    }
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.DrawImage(bmp, 0, 0);                  //This draws the fractal without resizing it
            e.Graphics.DrawImage(new Bitmap(bmp, new Size(this.Width, this.Height)), 0, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ResizeRedraw = true;
            DoubleBuffered = true;
            
            for (int i = 0; i < 1; i++)
            {
                randomLinearFractal(4, 1000000);
            }

            renderLoggedBitmap(visitedPixels, bmp, Color.Black, Color.White);
        }
    }

    struct Coordinates
    {
        public double x;
        public double y;

        public Coordinates(double xIn, double yIn)
        {
            x = xIn;
            y = yIn;
        }
    }
}
