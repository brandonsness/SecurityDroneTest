using System;
using System.Net.Sockets;
using System.Net;

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
                IPAddress ipAddress = IPAddress.Parse(servIP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                //Create TCP socket
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //connect
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                //get data from server
                GetSetupData(sender);

                //Take user input
                HandleInput(sender);

                //Release socket
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("Ooops, problem with DroneController connection " + e.ToString() + "\n");
            }
        }

        private void GetSetupData(Socket sock)
        {
            byte[] bytes = new byte[1024];
            try
            {
                //Get ClientKey
                sock.Receive(bytes);
                ClientKey = BitConverter.ToInt32(bytes);

                //Get Seed
                sock.Receive(bytes);
                Seed = DateTime.FromBinary(BitConverter.ToInt64(bytes)); 
            }
            catch(Exception e)
            {
                Console.WriteLine("Ooops, problem getting data from Drone" + e.ToString() + "\n");
            }
        }

        private void HandleInput(Socket sock)
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
                        sock.Send(BitConverter.GetBytes(encryptedInput));

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
