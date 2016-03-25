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

        double minX = -8;       //The range of R^2 to map to the screen
        double maxX = 8;
        double minY = -2;
        double maxY = 12;

        Random rand = new Random();
        int[,] visitedPixels = new int [ width,height];
        Bitmap bmp = new Bitmap(1440, 900);
        Graphics graphicsObj;
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
        void runProbGame(double[] initialPosition, double[][] inFunctions, double[] probs, int[,] visited, int iter)
        {
            double[] pos = initialPosition;
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

                //And here we reevaluate the position with the function choosen above and add one to our visited pixel store
                pos[0] = pos[0] * function[0] + pos[1] * function[1] + function[4];
                pos[1] = pos[0] * function[2] + pos[1] * function[3] + function[5];
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
                visited[x, y]++;
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
            runProbGame(initialPosition, fern, probs, visitedPixels, 1000000);

            //Here we draw the bitmap with the info from our visited pixels
            for(int x=0; x<1440; x++)
            {
                for(int y=0; y<900; y++)
                {
                    if(visitedPixels[x,y]!=0)
                    {
                        bmp.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        bmp.SetPixel(x, y, Color.Black);
                    }
                }
            }
            graphicsObj = Graphics.FromImage(bmp);
            graphicsObj.DrawImage(bmp, 0, 0);
            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.DrawImage(bmp, 0, 0);                  //This draws the fractal without resizing it
            e.Graphics.DrawImage(new Bitmap(bmp, new Size(this.Width, this.Height)), 0, 0);
        }
    }
}
