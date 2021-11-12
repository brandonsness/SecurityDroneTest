using System;
using System.Net.Sockets;

namespace SecurityDroneTest
{
    public class DroneController
    {
        public PRNG Rng { get; set; }

        private int ClientKey { get; set; }

        private DateTime Seed { get; set; }

        public DroneController(string servIP, int port)
        {
            HandleConnection(servIP, port);
        }

        private void HandleConnection(string servIP, int port)
        {
            try
            {
                TcpClient control = new TcpClient(servIP, port);

                NetworkStream stream = control.GetStream();

                Console.WriteLine("Socket connected");

                //get data from server
                GetSetupData(stream);

                //Take user input
                HandleInput(stream);

                //Release socket
                stream.Close();
                control.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("Ooops, problem with DroneController connection " + e.ToString() + "\n");
            }
        }

        private void GetSetupData(NetworkStream stream)
        {
            byte[] bytes = new byte[1024];
            try
            {
                //Get ClientKey
                stream.Read(bytes);
                ClientKey = BitConverter.ToInt32(bytes);
                Console.WriteLine("Client key is {0}", ClientKey);

                //Get Seed
                stream.Read(bytes);
                Seed = DateTime.FromBinary(BitConverter.ToInt64(bytes)); 
                Console.WriteLine("Seed is {0}", Seed);

                //Init Rng
                Rng = new PRNG(Seed);
            }
            catch(Exception e)
            {
                Console.WriteLine("Ooops, problem getting data from Drone" + e.ToString() + "\n");
            }
        }

        private void HandleInput(NetworkStream stream)
        {
            byte[] bytes = new byte[1024];
            while(true)
            {
                try
                {
                    //Get user input encrypt and send
                    try
                    {
                        //Will need to make input handle different for actual application
                        int input = Convert.ToInt32(Console.ReadLine());
                        int encryptedInput = EncryptorDecryptor.Encrypt(Rng.getNext(), input, ClientKey);

                        //Send encrypted data to Drone
                        stream.Write(BitConverter.GetBytes(encryptedInput));
                    }
                    catch(FormatException e)
                    {
                        //Again will need to rework how we handle input
                        Console.WriteLine("Invalid input: Please input a number\n");
                    }
                }

                catch(Exception e)
                {
                    Console.WriteLine("Ooops, problem sending data to Drone " + e.ToString() + "\n");
                }
            }

        }

    }
}
