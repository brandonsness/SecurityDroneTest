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
        //since we're using 5 floats thats 8 bytes * 5 = 40 bytes
        // 320 bits so 10 ints are needed in this byte array
        public byte[] getNext()
        {
            byte[] bytes = new byte[320];
            RNGMachine.NextBytes(bytes);

            return bytes; 
        }
    }
}
