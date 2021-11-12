using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SecurityDroneTest
{
    public class Drone
    {
        PRNG Rng { get; set; }

        int ClientKey { get; set; } 

        public Drone()
        {
            Rng = new PRNG();
            SetupConnection();
        }

        private void SetupConnection()
        {
            TcpListener drone = null;

            //Get first ipv4 addr 
            string externalIpString = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            IPAddress ipAddress = IPAddress.Parse(externalIpString);

            try
            {
                drone = new TcpListener(ipAddress, 0);
                drone.Start();

                Console.WriteLine("DRONE: Connected on {0}:{1}\n", ((IPEndPoint)drone.LocalEndpoint).Address.ToString(), ((IPEndPoint)drone.LocalEndpoint).Port.ToString());

                TcpClient control = drone.AcceptTcpClient();
                Console.WriteLine("Connected to controller\n");

                //Stream for communicating
                NetworkStream stream = control.GetStream();

                //TODO: Perform async key exchange here

                GenerateClientKey();

                //Send clientKey and Seed to controller
                stream.Write(BitConverter.GetBytes(ClientKey));
                stream.Write(BitConverter.GetBytes(Rng.Seed.Ticks));

                byte[] bytes = null;
                //listen for commands better way to do this
                while(true)
                {
                    bytes = new byte[1024];
                    stream.Read(bytes);
                    int data = BitConverter.ToInt32(bytes);
                    HandleCommand(data);
                }

                stream.Close();
                control.Close();
                drone.Stop();
            }
            catch(Exception e)
            {
                Console.WriteLine("Oops Drone encountered error" + e.ToString() + "\n");
            }
        }

        private bool HandleCommand(int input)
        {
            int command = EncryptorDecryptor.Decrypt(Rng.getNext(), input, ClientKey);
            Console.WriteLine("DRONE: Decrypted Command is: " + command + "\n");
            return true;
        }

        private void GenerateClientKey()
        {
            //TODO: Generate client key
            //For protoyping stage will set it to fixed number;
            ClientKey = 434823234;
        }
    }
}
