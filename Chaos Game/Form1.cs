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
        const int width = 2880;
        const int height = 1800;

        Random rand = new Random();
        int[,] visitedPixels = new int [width,height];
        Bitmap bmp = new Bitmap(width, height);

        //Takes a list of functions, with the same form as runGame's functions, and a list of cumulative probabilities for the functions and iterates with them
        int runProbGame(double[] initialPosition, double[][] inFunctions, double[] probs, int[,] visited, int iter, double[] window)
        {
            double minX = window[0];
            double maxX = window[1];
            double minY = window[2];
            double maxY = window[3];
            int numVisited = 0;
            //double averageDist = 0.0;
            double[] pos = new double[] { initialPosition[0], initialPosition[1]};
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
                        visited[x, y]++;
                    }

                    
                }

                //double dist = Math.Pow(pos[0] * pos[0] + pos[1] * pos[1], .5);
                //averageDist = averageDist*(i)/(i+1)+dist/(i+1);
            }
            //Console.WriteLine(averageDist); //Uncomment this line to write the average distance of each point from the origin in the fractal
            return numVisited;
        }

        //Returns number of distinct pixels visited in the window
        int randomLinearFractal(int functionNumber, int iterations, int[,] store, double[] window) 
        {
            double[][] functions = new double[functionNumber][];
            double[] pos = { 0, 0 };
            double[] probs = new double[functionNumber];
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
                probs[p] = (1.0 + p) / functionNumber;
            }
            return runProbGame(pos, functions, probs, store, iterations, window);
        }

        //Writes pixelData from int[,] to a bitmap
        void renderBinaryBitmap(int[,] dataSource, Bitmap drawingMap, Color offColor, Color onColor)    
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
        
        double convertStringToDouble(String str, double min, double max)
        {
            UInt64 val = Convert.ToUInt64(str, 2);
            double valMax = Math.Pow(2, str.Length) - 1.0;
            return val / valMax * (max - min) + min;
        }

        void generateFunctionListFromGenome(Genetics.Genome inGenome, int numFunctions, int bitsPerVar, double[][] functions)
        {
            for (int f = 0; f < numFunctions; f++)
            {
                for (int v = 0; v < 6; v++)
                {
                    functions[f][v] = convertStringToDouble(inGenome.genes.Substring(bitsPerVar * (v + 6 * f), bitsPerVar), -1, 1);
                }
            }
        }

        Genetics.Genome[] reproduce(Genetics.Genome[] inGenomes, int[] inPixelsHit, double dblRate, double mutRate)
        {
            Array.Sort(inPixelsHit, inGenomes);
            Array.Reverse(inGenomes);
            Array.Sort(inPixelsHit);

            Genetics.Genome[] children = new Genetics.Genome[inGenomes.Length];

            //Keep 20% of the best fractals
            for (int i = 0; i < (int)(children.Length * .2); i++)
            {
                children[i] = inGenomes[i];
            }

            //For the other 80%, choose two of the best 20% and make a child
            for (int i = (int)(children.Length * .2); i < children.Length; i++)
            {
                //Randomly choose parents from the top 20%
                int randFatherIndex = rand.Next(0, (int)(children.Length * .2));
                int randMotherIndex = rand.Next(0, (int)(children.Length * .2));

                //Create a genome for the child from the genomes of the parents
                Genetics.Genome child = inGenomes[randFatherIndex].offspring(inGenomes[randMotherIndex], dblRate, mutRate);
                children[i] = child;
            }
            return children;
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

            double[] initialPosition = { 0, 0 };
            double[] standardWindow = { -2, 2, -2, 2 };

            const int numFunctions = 10;                 //Number of functions in each fractal
            const int fractalPoolSize = 80;             //Number of fractals generated in each generation
            const double dblRate = .5;
            const double mutRate = .02;                 //Mutation rate during evolution
            const int precisionOfGenomeVariables = 16;  //Bits of precision for the variables in our genomes

            //*************************************************************
            //Lower pixelsToPlot and goalPixelsOnScreen to speed up program
            //*************************************************************

            const int goalPixelsOnScreen = 50000;       //Fractal evolution will proceed for this many generations
            const int pixelsToPlot = 100000;            //or until a fractal has goalPixels in the window after pixelsToPlot iterations
            const int generations = 50;                 //

            int[] pixelsHit = new int[fractalPoolSize];
            double[][] functions = new double[numFunctions][];
            double[] probs = new double[numFunctions];
            Genetics.Genome[] genomes = new Genetics.Genome[fractalPoolSize];
            int numVarsInGenome = 6 * numFunctions; //Number of variables in each genome
            
            for (int i=0; i<numFunctions; i++)      //Sets up cumulative probabilities array for the fractals
            {
                probs[i] = (i + 1.0) / numFunctions;
            }
            for(int i=0; i<fractalPoolSize; i++)    //Initialize the fractals' genomes randomly
            {
                genomes[i] = new Genetics.Genome(numVarsInGenome * precisionOfGenomeVariables);
            }
            for (int i = 0; i < numFunctions; i++)  //Initialize functions to be empty
            {
                functions[i] = new double[6];
            }

            
            int maxPixels = 0;      //This max is used to decide when to render a new bitmap
            int maxPixelsSeen = 0;  //This max is used to log information, it is not used in any calculations
            for(int gen=1; gen<generations+1 && maxPixels< goalPixelsOnScreen; gen++)
            {
                for (int i = 0; i < genomes.Length; i++)
                {
                    Array.Clear(visitedPixels, 0, visitedPixels.Length);
                    generateFunctionListFromGenome(genomes[i], numFunctions, precisionOfGenomeVariables, functions);
                    pixelsHit[i] = runProbGame(initialPosition, functions, probs, visitedPixels, pixelsToPlot, standardWindow);
                    if ((pixelsHit[i] > maxPixels && gen == generations) || pixelsHit[i] >= goalPixelsOnScreen)
                    {
                        maxPixels = pixelsHit[i];
                        Console.WriteLine(pixelsHit[i]);
                        renderBinaryBitmap(visitedPixels, bmp, Color.Black, Color.White);
                    }
                    if (pixelsHit[i] > maxPixelsSeen)
                    {
                        maxPixelsSeen = pixelsHit[i];
                    }
                    if (pixelsHit[i] >= goalPixelsOnScreen)
                    {
                        Console.WriteLine("Found satisfactory fractal on generation: {0:d}, fractal number: {1:d} of the generation", gen, i+1);
                        break;
                    }
                }
                Console.WriteLine("{0:d}, {1:d}/{2:d}", gen, maxPixelsSeen, goalPixelsOnScreen);
                genomes=reproduce(genomes, pixelsHit, dblRate, mutRate);
            }
        }

        //Draws the barnsley fern
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

        //Draws a bunch of fractals that look like galaxies
        void runGalaxy(int galaxies, int points, int[,] store, double[] window)
        {
            for (int i = 0; i < galaxies; i++)
            {
                randomLinearFractal(1, points, store, window);
            }
        }
    }
}
