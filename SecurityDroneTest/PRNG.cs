using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityDroneTest
{
    class PRNG
    {


        /* Remove comment to allow for testing
        static void Main(string[] args)
        {
            for(int i = 0; i < 10; i++)
            {
                Console.WriteLine(getNumber().Next());
                Console.WriteLine(getNumber().Next());
                Console.WriteLine(" ");
            }
            Console.ReadLine();
        }
        */
        private static DateTime localDate = DateTime.Now;
        private static int sec = localDate.Second;
        private static int numbCalls = 1;

        public static Random getNumber()
        {
            numbCalls++;
            int Fin = numbCalls * sec;
            Random numb = new Random(Fin);
            return numb;
        }
    }
}
