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

        /*double minX = -8;       //The range of R^2 to map to the screen
        double maxX = 8;          //This window is fit to the barnsley fern
        double minY = -2;
        double maxY = 12;*/

        double minX = -2;       //The range of R^2 to map to the screen
        double maxX = 2;        //This window seems to work well for random fractals
        double minY = -2;
        double maxY = 2;

        Random rand = new Random();
        int[,] visitedPixels = new int [ width,height];
        Bitmap bmp = new Bitmap(1440, 900);
        public Form1()
        {
            InitializeComponent();
        }

        //Takes a single function of the form
        //          [ 0  1 ] [x] + [4]
        //          [ 2  3 ] [y]   [5]
        //and iterates it
        void runGame(double[] initialPosition, double[] arr, int[,] visited, int iter)
        {
            double[] pos = initialPosition;
            int x=0, y=0;
            for (int i = 0; i < iter; i++)
            {
                pos[0] = pos[0] * arr[0] + pos[1] * arr[1] + arr[2];
                pos[1] = pos[0] * arr[3] + pos[1] * arr[4] + arr[5];
                if(pos[0]<=1&&pos[0]>=-1)
                {
                    x=(int)(pos[0] * width / 2 + width / 2);
                }
                if(pos[1]<=1&&pos[1]>=-1)
                {
                    y=(int)(pos[1] * height / 2 + height / 2);
                }
                visited[x, y]++;
            }
        }

        //Takes a list of functions, with the same form as runGame's functions, and a list of cumulative probabilities for the functions and iterates with them
        int runProbGame(double[] initialPosition, double[][] inFunctions, double[] probs, int[,] visited, int iter)
        {
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

                if (pos[0] <= maxX && pos[0] >= minX)
                {
                    double normalizedX = (pos[0] - minX) / (maxX - minX);
                    x = (int)(normalizedX * width);
                }
                if (pos[1] <= maxY && pos[1] >= minY)
                {
                    double normalizedY = (pos[1] - minY) / (maxY - minY);
                    y = (int)(height - normalizedY * height);
                }
                if(visited[x, y]==0)
                {
                    visited[x, y]++;
                    numVisited++;
                }
                visited[x, y]++;
                double dist = Math.Pow(pos[0] * pos[0] + pos[1] * pos[1], .5);
                averageDist = averageDist*(i)/(i+1)+dist/(i+1);
            }
            //Console.WriteLine(averageDist); //Uncomment this line to write the average distance of each point from the origin in the fractal
            return numVisited;
        }

        int randomLinearFractal(int functionNumber, int iterations, int[,] store) //Returns number of distinct pixels visited on the screen
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
                probs[p] = 1.0 / functionNumber;
            }
            return runProbGame(pos, functions, probs, store, iterations);
        }

        void renderBinaryBitmap(int[,] dataSource, Bitmap drawingMap, Color offColor, Color onColor)    //Stores data in dataSource into the drawingMap Bitmap
        {
            for(int x=0; x<dataSource.GetLength(0); x++)
            {
                for(int y=0; y<dataSource.GetLength(1); y++)
                {
                    if(dataSource[x,y]==0)
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

        private void Form1_Load(object sender, EventArgs e)
        {
            ResizeRedraw = true;
            DoubleBuffered = true;

            double[][] fern = new double[4][];              //For layout of these matricies see the comment above runGame
            fern[0] = new double[6] { 0, 0, 0, .16, 0, 0};
            fern[1] = new double[6] { .85, .04, -.04, .85, 0, 1.60};
            fern[2] = new double[6] { .20, -.26, .23, .22, 0, 1.60};
            fern[3] = new double[6] { -.15, .28, .26, .24, 0, .44 };
            double[] probs = { .01, .86, .93, 1.0};
            double[] initialPosition = { 0, 0};

            //runProbGame(initialPosition, fern, probs, visitedPixels, 1000000);    //Uncomment to run fern fractal
            randomLinearFractal(5, 10000, visitedPixels);

            renderBinaryBitmap(visitedPixels, bmp, Color.Black, Color.White);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.DrawImage(bmp, 0, 0);                  //This draws the fractal without resizing it
            e.Graphics.DrawImage(new Bitmap(bmp, new Size(this.Width, this.Height)), 0, 0);
        }
    }
}
