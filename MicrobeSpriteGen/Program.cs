/******************************************************************************
 * Cody Brock
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MicrobeSpriteGen
{
    class Program
    {
        public static int X_DIM = 128;
        public static int Y_DIM = 128;
        public static Point CENTER = new Point(X_DIM / 2, Y_DIM / 2);
        public static double MAX_RADIUS_FACTOR = 2;
        public static double MIN_RADIUS_FACTOR = 0.5;
        public static int LINE_WEIGHT = 3;
        public static Color COLOR_1 = Color.Green;
        public static Color COLOR_2 = Color.Yellow;
        public static Color COLOR_3 = Color.Red;
        public static Color COLOR_4 = Color.LightBlue;

        static void Main(string[] args)
        {

            Random rand = new Random();

            /*
            for (int j = 3; j <= 15; j++)
                for (double i = 0.0; i <= 0.1; i += 0.01)
                    TestBatch(rand, "verts"+j+"_radVar"+i, 20, 40, 0, i, j);
            */

            List<Genome> genomes = new List<Genome>();
            for (int i = 0; i < 10; i++)
            {
                Genome genome = new Genome();
                genome.print();
                GenSprite(rand, genome).Save("microbe" + (i + 1) + "_A" + genome.A + "_G" + genome.G + "_T"+genome.T+"_C"+genome.C+".png");
                genomes.Add(genome);
            }
            

        }

        public static Bitmap GenSprite(Random rand, Genome genome)
        {
            // calculate cell wall parameters
            int aveRad      = genome.A + genome.G;                                                  // 20-40        A+G
            double angVar   = .5 * Math.Tanh(genome.C / genome.T - 1) + .5;                         // 0.0-1.0      C:T
            double radVar   = .2 * Math.Tanh(4.0/5.0*(genome.C / genome.T) - 1) + .2;               // 0.00-0.15    A:G

            int max = Math.Max(Math.Max(genome.A, genome.G), Math.Max(genome.T, genome.C));
            int min = Math.Min(Math.Min(genome.A, genome.G), Math.Min(genome.T, genome.C));

            int verts       = Convert.ToInt32(8 * Math.Tanh(max/min / 16 - 1) + 10);                // 4-16         MAX:MIN

            // determine base color
            int r = 0;
            int g = 0;
            int b = 0;
            // attack > defense -> RED
            if (genome.A >= genome.G)   
            {
                r = 255;
                // buff > debuff -> ORANGE
                if (genome.T >= genome.C)   
                    g = 150 * (genome.T / genome.A);
                // debuff > buff -> MAGENTA
                else
                    b = 200 * (genome.C / genome.A);
            }
            // defense > attack -> GREEN
            else
            {
                g = 255;
                // debuff > buff -> TEAL
                if (genome.C >= genome.T)
                    b = 200 * (genome.C / genome.G);
                // buff > debuff -> YELLOW
                else
                {
                    b = 100;
                    r = 100 + 155 * (genome.T / genome.G);
                }      
            }

            Color baseColor = Color.FromArgb(255, r, g, b);

            // generate cell wall polygon
            Point[] wallPoints = GenPolygonPoints(rand, 0.25, CENTER, aveRad, angVar, radVar, verts);
            wallPoints = ExpandPoints(wallPoints, CENTER);

            Bitmap bmp = new Bitmap(X_DIM, Y_DIM);
            Graphics graphics = Graphics.FromImage(bmp);
            Pen pen1 = new Pen(Color.DarkGray, LINE_WEIGHT);
            Brush brush1 = new SolidBrush(baseColor);
            //Pen pen2 = new Pen(COLOR_3, LINE_WEIGHT);
            //Brush brush2 = new SolidBrush(COLOR_4);
            graphics.FillPolygon(brush1, wallPoints);
            graphics.DrawPolygon(pen1, wallPoints);
            //graphics.FillPolygon(brush2, points2);
            //graphics.DrawPolygon(pen2, points2);
            //bmp.Save(name + "\\sample_" + (i + 1) + ".png");

            return bmp;

        }

        public static void TestBatch(Random rand, string name, int runs, int radius, double angVar, double radVar, int verts)
        {
            name = "tests\\" + name;

            Console.WriteLine("#############################################");
            Console.WriteLine("TEST BATCH: " + name);
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("Runs            | " + runs);
            Console.WriteLine("Avg. Radius     | " + radius);
            Console.WriteLine("Angle variance  | " + angVar);
            Console.WriteLine("Radial variance | " + radVar);
            Console.WriteLine("Vertices/quad   | " + verts);
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine(" Creating directory...");
            
            // create directory for batch
            try
            {
                // Determine whether the directory exists.
                int suffix = 2;
                string original = name;
                while (Directory.Exists(name))
                {
                    name = original + suffix;
                    suffix++;
                }
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(name);
                Console.WriteLine("The directory \"{0}\" was created successfully.", name);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
                Console.WriteLine("Exiting...");
                Console.ReadLine();
                return;
            }
            finally { }

            Console.WriteLine("Generating images...");
            // generate test images
            for (int i = 0; i < runs; i++)
            {
                // generate cell wall polygon
                Point[] quadrant = GenPolygonPoints(rand, 0.25, CENTER, radius, angVar, radVar, verts);
                Point[] points = ExpandPoints(quadrant, CENTER);
                // generate nucleus polygon
                Point[] quadrant2 = GenPolygonPoints(rand, 0.25, CENTER, radius/3, angVar, radVar, verts);
                Point[] points2 = ExpandPoints(quadrant2, CENTER);
                // draw polygon
                Bitmap bmp = new Bitmap(X_DIM, Y_DIM);
                Graphics graphics = Graphics.FromImage(bmp);
                Pen pen1 = new Pen(COLOR_1, LINE_WEIGHT);
                Brush brush1 = new SolidBrush(COLOR_2);
                Pen pen2 = new Pen(COLOR_3, LINE_WEIGHT);
                Brush brush2 = new SolidBrush(COLOR_4);
                graphics.FillPolygon(brush1, points);
                graphics.DrawPolygon(pen1, points);
                graphics.FillPolygon(brush2, points2);
                graphics.DrawPolygon(pen2, points2);
                bmp.Save(name + "\\sample_" + (i + 1) + ".png");
            }
            Console.WriteLine("Test batch complete.\n");
        }

        public static Point[] ExpandPoints(Point[] quadrant, Point center)
        {
            Point[] points = new Point[quadrant.Length * 4];

            int j = 0;
            for (int i = 0; i < quadrant.Length; i++)
            {
                points[j] = quadrant[i];
                j++;
            }
            for (int i = quadrant.Length - 1; i >= 0; i--)
            {
                points[j] = new Point(points[i].X - 2 * (points[i].X - center.X), points[i].Y);
                j++;
            }
            for (int i = 0; i < quadrant.Length; i++)
            {
                points[j] = new Point(points[i].X - 2 * (points[i].X - center.X), points[i].Y - 2 * (points[i].Y - center.Y));
                j++;
            }
            for (int i = quadrant.Length - 1; i >= 0; i--)
            {
                points[j] = new Point(points[i].X, points[i].Y - 2 * (points[i].Y - center.Y));
                j++;
            }

            return points;
        }


        // https://stackoverflow.com/questions/8997099/algorithm-to-generate-random-2d-polygon
        // Adapted from answer by @Mike Ounsworth
        // slice        | portion of circle to generate points for (i.e. 1 = full circle, 0.25 = quarter circle)
        // irregularity | 0.0 - 1.0 (affects the angle between vertices
        // spikeyness   | 0.0 - 1.0 (affects how close vertices are to the average radius)
        public static Point[] GenPolygonPoints(Random rand, double slice, Point center, int aveRadius, double irregularity, double spikeyness, int numVerts)
        {
            slice = slice * 2 * Math.PI;
            irregularity = irregularity * slice / numVerts;
            spikeyness = spikeyness * aveRadius;

            // angle steps
            List<double> angleSteps = new List<double>();
            double lower = (slice / numVerts) - irregularity;
            double upper = (slice / numVerts) + irregularity;
            double sum = 0;
            for (int i = 0; i < numVerts + 1; i++)
            {
                double tmp = rand.NextDouble() * (upper - lower) + lower;
                angleSteps.Add(tmp);
                sum += tmp;
            }

            // normalize to fit slice
            double k = sum / slice;
            for (int i = 0; i < numVerts; i++)
                angleSteps[i] = angleSteps[i] / k;

            // generate points
            List<Point> points = new List<Point>();
            double angle = 0;
            for (int i = 0; i < numVerts; i++)
            {
                angle += angleSteps[i];
                double r_i = Gauss(rand, aveRadius, spikeyness);
                if (r_i > MAX_RADIUS_FACTOR * aveRadius)
                    r_i = MAX_RADIUS_FACTOR * aveRadius;
                if (r_i < MIN_RADIUS_FACTOR * aveRadius)
                    r_i = MIN_RADIUS_FACTOR * aveRadius;
                double x = center.X + r_i * Math.Cos(angle);
                double y = center.Y + r_i * Math.Sin(angle);
                points.Add(new Point(Convert.ToInt32(Math.Round(x)), Convert.ToInt32(Math.Round(y))));
            }

            return points.ToArray();
        }

        // https://stackoverflow.com/questions/218060/random-gaussian-variables
        // Adapted from answer by @yoyoyoyosef
        public static double Gauss(Random rand, double mean, double stdDev)
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        }

        

    }
}
