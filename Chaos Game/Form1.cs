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
        int[,] visitedPixels = new int [ width,height];
        Bitmap bmp = new Bitmap(1440, 900);

        //Takes a list of functions, with the same form as runGame's functions, and a list of cumulative probabilities for the functions and iterates with them
        int runProbGame(double[] initialPosition, double[][] inFunctions, double[] probs, int[,] visited, int iter, double[] window)
        {
            double minX = window[0];
            double maxX = window[1];
            double minY = window[2];
            double maxY = window[3];
            int numVisited = 0;
            double averageDist = 0.0;
            double[] pos = initialPosition;
            double[] temp = {0, 0};
            double[] function= {0, 0, 0, 0, 0,0};
            int x = 0, y = 0;
            for(int i=0; i< iter; i++)
            {
                //Here we get a function from the list
                double p = rand.NextDouble();
                if(p<=probs[0])
                {
                    function = inFunctions[0];
                }
                else
                {
                    for(int funcTest=1; funcTest<probs.Length; funcTest++)
                    {
                        if(p<probs[funcTest] && p>probs[funcTest-1])
                        {
                            function = inFunctions[funcTest];
                            break;
                        }
                    }
                }

                temp[0] = pos[0] * function[0] + pos[1] * function[1] + function[4];
                temp[1] = pos[0] * function[2] + pos[1] * function[3] + function[5];
                pos[0] = temp[0];
                pos[1] = temp[1];

                if (pos[0] <= maxX && pos[0] >= minX && pos[1] <= maxY && pos[1] >= minY)
                {
                    double normalizedX = (pos[0] - minX) / (maxX - minX);
                    x = (int)(normalizedX * width);

                    double normalizedY = (pos[1] - minY) / (maxY - minY);
                    y = (int)(height - normalizedY * height);

                    if(visited[x, y] == 0)
                    {
                        numVisited++;
                    }

                    visited[x, y]++;
                }

                double dist = Math.Pow(pos[0] * pos[0] + pos[1] * pos[1], .5);
                averageDist = averageDist*(i)/(i+1)+dist/(i+1);
            }
            //Console.WriteLine(averageDist); //Uncomment this line to write the average distance of each point from the origin in the fractal
            return numVisited;
        }

        int randomLinearFractal(int functionNumber, int iterations, int[,] store, double[] window) //Returns number of distinct pixels visited on the screen
        {
            double[][] functions = new double[functionNumber][];
            double[] pos = { 0, 0 };
            double[] probs = new double[functionNumber];
            double[] temp = { 0, 0 };
            for (int f = 0; f < functionNumber; f++)
            {
                functions[f] = new double[6];
                for (int c = 0; c < 6; c++)
                {
                    functions[f][c] = 2*rand.NextDouble()-1.0;
                }
            }
            for (int p = 0; p < functionNumber; p++)
            {
                probs[p] = (1 + p) * 1.0 / functionNumber;
            }
            return runProbGame(pos, functions, probs, store, iterations, window);
        }

        void runFern(int[,] dataStore, int iterations)
        {
            double[] window = { -8, 8, -2, 12 };
            double[][] fern = new double[4][];              //For layout of these matricies see the comment above runGame
            fern[0] = new double[6] { 0, 0, 0, .16, 0, 0 };
            fern[1] = new double[6] { .85, .04, -.04, .85, 0, 1.60 };
            fern[2] = new double[6] { .20, -.26, .23, .22, 0, 1.60 };
            fern[3] = new double[6] { -.15, .28, .26, .24, 0, .44 };
            double[] probs = { .01, .86, .93, 1.0 };
            double[] initialPosition = { 0, 0 };

            runProbGame(initialPosition, fern, probs, dataStore, iterations, window);
        }

        void runGalaxy(int galaxies, int points, int[,] store, double[] window)
        {
            for(int i=0; i<galaxies; i++)
            {
                randomLinearFractal(1, points, store, window);
            }
        }

        void renderBinaryBitmap(int[,] dataSource, Bitmap drawingMap, Color offColor, Color onColor)    //Stores data in dataSource into the drawingMap Bitmap
        {
            for (int x = 0; x < dataSource.GetLength(0); x++)
            {
                for (int y = 0; y < dataSource.GetLength(1); y++)
                {
                    if (dataSource[x, y] == 0)
                    {
                        drawingMap.SetPixel(x, y, offColor);
                    }
                    else
                    {
                        drawingMap.SetPixel(x, y, onColor);
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

            double[] standardWindow = { -2, 2, -2, 2 };
            //runFern(visitedPixels, 1000000);
            //runGalaxy(300, 10000, visitedPixels, standardWindow);

            renderBinaryBitmap(visitedPixels, bmp, Color.Black, Color.White);
        }
    }
}
