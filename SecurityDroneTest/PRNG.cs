using System;

namespace SecurityDroneTest
{
    public class PRNG
    {
        public DateTime Seed { get; private set; }

        private Random RNGMachine { get; set; } 
       
        public PRNG()
        {
            Seed = DateTime.Now;
            RNGMachine = new Random(Seed.Second);
        }

        public PRNG(DateTime seed)
        {
            Seed = seed;
            RNGMachine = new Random(Seed.Second);
        }

        public int getNext()
        {
            return RNGMachine.Next();
        }
    }
}
