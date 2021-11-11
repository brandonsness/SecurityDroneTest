using System;

namespace SecurityDroneTest
{
    public class Tester
    {
        public static void Main(string [] args)
        {
            // Demonstrating public/private key generation. Can delete later.
            DroneRSA keys = new DroneRSA();
            Console.WriteLine(BitConverter.ToString(keys.rsaKeyInfo.Exponent));
            Console.WriteLine(BitConverter.ToString(keys.rsaKeyInfo.D));

            Drone drone = new Drone();
            //DroneController control = new DroneController();

        }
    }
}
