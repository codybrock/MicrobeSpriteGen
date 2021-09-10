using System;
using System.Collections.Generic;
using System.Text;

namespace MicrobeSpriteGen
{
    class Genome
    {
        public static int LENGTH = 60;
        public static int MAX_INIT = 60;    //30;
        public static int MIN_INIT = 0;     //10;
        public string Sequence { get; set; }
        public int A { get; set; }
        public int G { get; set; }
        public int T { get; set; }
        public int C { get; set; }

        public Genome()
        {
            Random rand = new Random();
            while (true)
            {
                Sequence = "";
                A = 0;
                G = 0;
                T = 0;
                C = 0;
                for (int i = 0; i < LENGTH; i++)
                {
                    int caseSwitch = rand.Next(0, 4);
                    // generate DNA sequence
                    switch (caseSwitch)
                    {
                        // 0: a
                        case 0:
                            Sequence += "a";
                            A++;
                            break;
                        // 1: g
                        case 1:
                            Sequence += "g";
                            G++;
                            break;
                        // 2: t
                        case 2:
                            Sequence += "t";
                            T++;
                            break;
                        // 3: c 
                        case 3:
                            Sequence += "c";
                            C++;
                            break;
                        // unexpected value
                        default:
                            break;
                    }
                    // stop generating if one nucleotide exceeds max allowed
                    if (A > MAX_INIT || G > MAX_INIT || T > MAX_INIT || C > MAX_INIT)
                        break;
                }
                // make sure sequence is correct length (meaning we didn't break early)
                // as long as all nucleotides have at least the min, accept the sequence.
                if (Sequence.Length == LENGTH)
                    if (A >= MIN_INIT && G >= MIN_INIT && T >= MIN_INIT && C >= MIN_INIT)
                        return;
            }
        }

        public void print()
        {
            Console.WriteLine("GENOME: " + Sequence);
            Console.WriteLine("     A: " + A + " (" + Math.Round((double)A / LENGTH * 100, 2) + "%)");
            Console.WriteLine("     G: " + G + " (" + Math.Round((double)G / LENGTH * 100, 2) + "%)");
            Console.WriteLine("     T: " + T + " (" + Math.Round((double)T / LENGTH * 100, 2) + "%)");
            Console.WriteLine("     C: " + C + " (" + Math.Round((double)C / LENGTH * 100, 2) + "%)");
        }
    }
}
