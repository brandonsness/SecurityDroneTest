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
            //Get first ipv4 addr and set port as 0 for any available port
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 0);

            try
            {
                //Create socket
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //bind to socket
                sock.Bind(localEndPoint);

                //only want one client
                sock.Listen(1);

                //Accept connection
                Socket serv = sock.Accept();

                //TODO: Perform async key exchange here

                GenerateClientKey();

                //Send clientKey and Seed to controller
                serv.Send(BitConverter.GetBytes(ClientKey));
                serv.Send(BitConverter.GetBytes(Rng.Seed.Ticks));

                byte[] bytes = null;
                //listen for commands better way to do this
                while(true)
                {
                    bytes = new byte[1024];
                    int data = BitConverter.ToInt32(bytes);
                    HandleCommand(data);
                }

                serv.Shutdown(SocketShutdown.Both);
                serv.Close();

            }
            catch(Exception e)
            {
                Console.WriteLine("Oops Drone encountered error" + e + "\n");
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
