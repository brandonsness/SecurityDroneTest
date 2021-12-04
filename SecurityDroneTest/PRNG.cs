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
            RNGMachine = new Random(Seed.Millisecond);
        }

        public PRNG(DateTime seed)
        {
            Seed = seed;
            RNGMachine = new Random(Seed.Millisecond);
        }

        //gets byte array for next drone instruction
        //since we're using 4 doubles thats 8 bytes * 4 = 32 bytes
        // 256 bits so 10 ints are needed in this byte array
        public byte[] getNext()
        {
            byte[] bytes = new byte[32];
            RNGMachine.NextBytes(bytes);

            return bytes; 
        }
    }
}
