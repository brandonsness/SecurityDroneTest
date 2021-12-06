using System;

namespace SecurityDroneTest
{
    /// <summary>
    /// Our Helper class that handles our Psuedo Random number generation
    /// </summary>
    public class PRNG
    {
        /// <summary>
        /// Seed used to start the PRNG
        /// </summary>
        public DateTime Seed { get; private set; }

        /// <summary>
        /// RNG machine used to generate psuedo random numbers
        /// </summary>
        private Random RNGMachine { get; set; }

        /// <summary>
        /// Default Constructer for our PRNG Uses the current time in milliseconds as a seed
        /// </summary>
        public PRNG()
        {
            Seed = DateTime.Now;
            RNGMachine = new Random(Seed.Millisecond);
        }

        /// <summary>
        /// Constructor used by Controller to start it's PRNG
        /// </summary>
        /// <param name="seed">Seed used to start PRNG between Drone and Controller on same path</param>
        public PRNG(DateTime seed)
        {
            Seed = seed;
            RNGMachine = new Random(Seed.Millisecond);
        }

        /// <summary>
        /// Gets the next set of psuedo random numbers used in our encryption scheme
        /// </summary>
        /// <returns>byte[] of size 32</returns>
        public byte[] getNext()
        {
            //since we're using 4 doubles thats 8 bytes * 4 = 32 bytes
            // 256 bits so 10 ints are needed in this byte array
            byte[] bytes = new byte[32];
            RNGMachine.NextBytes(bytes);

            return bytes;
        }
    }
}
