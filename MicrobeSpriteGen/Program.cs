using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MicrobeSpriteGen
{
    class Program
    {
        public static int X_DIM = 512;
        public static int Y_DIM = 512;
        public static double MAX_RADIUS_FACTOR = 2;
        public static double MIN_RADIUS_FACTOR = 0.5;

        static void Main(string[] args)
        {
            Random rand = new Random();
            Console.Write("Batch name: ");
            string batch = Console.ReadLine();
            Console.Write("# Samples: ");
            int samples = Int32.Parse(Console.ReadLine());

            Console.WriteLine("\nGenerating...");

            // create directory for batch
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(batch))
                {
                    Console.WriteLine("That path exists already.");
                    return;
                }
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(batch);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(batch));
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }

            // generate test images
            for (int i = 0; i < samples; i++)
                gen2(rand, X_DIM/2-1, Y_DIM/2-1, 128, 0, 1.0/samples*i, 10).Save(batch + "\\" + (i + 1) + ".png");
                //generate(rand).Save(batch + "\\" + (i+1) + ".png");

            Console.WriteLine("\nDone!");
            Console.ReadLine();

        }

        public static Bitmap generate(Random rand)
        {
            Bitmap bmp = new Bitmap(32, 32);
            Graphics graphics = Graphics.FromImage(bmp);
            Pen pen = new Pen(Color.Blue, 1);

            int start = rand.Next(32);
            int end = rand.Next(32);
            graphics.DrawLine(pen, 0, start, 31, end);

            return bmp;
        }

        // https://stackoverflow.com/questions/8997099/algorithm-to-generate-random-2d-polygon
        // Adapted from answer by @Mike Ounsworth
        public static Bitmap gen2(Random rand, int ctrX, int ctrY, int aveRadius, double irregularity, double spikeyness, int numVerts)
        {
            // both irregularity and spikeyness should be between 0.0 and 1.0
            irregularity = irregularity * 2 * Math.PI / numVerts;
            spikeyness = spikeyness * aveRadius;

            // angle steps
            List<double> angleSteps = new List<double>();
            double lower = (2 * Math.PI / numVerts) - irregularity;
            double upper = (2 * Math.PI / numVerts) + irregularity;
            double sum = 0;
            for (int i = 0; i < numVerts; i++)
            {
                double tmp = rand.NextDouble() * (upper - lower) + lower;
                angleSteps.Add(tmp);
                sum += tmp;
            }
            // normalize
            double k = sum / (2 * Math.PI);
            for (int i = 0; i < numVerts; i++)
                angleSteps[i] = angleSteps[i] / k;

            // generate points
            List<Point> points = new List<Point>();
            double angle = rand.NextDouble() * 2 * Math.PI;
            for (int i = 0; i < numVerts; i++)
            {
                double r_i = gauss(rand, aveRadius, spikeyness);
                if (r_i > MAX_RADIUS_FACTOR * aveRadius)
                    r_i = MAX_RADIUS_FACTOR * aveRadius;
                if (r_i < MIN_RADIUS_FACTOR * aveRadius)
                    r_i = MIN_RADIUS_FACTOR * aveRadius;
                double x = ctrX + r_i * Math.Cos(angle);
                double y = ctrY + r_i * Math.Sin(angle);
                points.Add(new Point(Convert.ToInt32(Math.Round(x)), Convert.ToInt32(Math.Round(y))));
                angle += angleSteps[i];
            }

            // draw polygon
            Bitmap bmp = new Bitmap(X_DIM, Y_DIM);
            Graphics graphics = Graphics.FromImage(bmp);
            Pen pen = new Pen(Color.Blue, 1);
            graphics.DrawPolygon(pen,points.ToArray());

            return bmp;
        }

        // https://stackoverflow.com/questions/218060/random-gaussian-variables
        // Adapted from answer by @yoyoyoyosef
        public static double gauss(Random rand, double mean, double stdDev)
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        }
    }
}
