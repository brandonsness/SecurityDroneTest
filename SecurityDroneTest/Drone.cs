using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    public class Drone
    {
        PRNG Rng { get; set; }

        byte [] ClientKey { get; set; } 

        public Drone()
        {
            Rng = new PRNG();
            SetupConnection();
        }

        private void SetupConnection()
        {
            TcpListener drone = null;

            //Get first ipv4 addr 
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

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
                ShareClientKey(stream);

                //Send clientKey and Seed to controller
                stream.Write(BitConverter.GetBytes(Rng.Seed.Ticks));

                byte[] bytes = null;
                //listen for commands better way to do this
                while(true)
                {
                    bytes = new byte[32];
                    stream.Read(bytes);
                    HandleCommand(bytes);
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

        private bool HandleCommand(byte [] input)
        {
            byte [] command = EncryptorDecryptor.Decrypt(Rng.getNext(), input, ClientKey);
            double[] decrypt = GetDoubles(command);
            Console.WriteLine("DRONE: Decrypted Command is: {0},{1},{2},{3}", decrypt[0], decrypt[1], decrypt[2], decrypt[3]);
            return true;
        }

        private void GenerateClientKey()
        {
            //Use new Cryptographic RNG for ClientKey 
            byte[] bytes = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            ClientKey = bytes;
        }

        private void ShareClientKey(NetworkStream stream)
        {
            Console.WriteLine("Client Key is {0}\n", Encoding.Default.GetString(ClientKey));
            RSA enc = new RSACryptoServiceProvider();
            byte[] cPubKey = new byte[4096];
            //recv client public key
            stream.Read(cPubKey);
            //encrypt the ClientKey with client's public key
            int tmp;
            enc.ImportRSAPublicKey(cPubKey, out tmp);
            stream.Write(enc.Encrypt(ClientKey, RSAEncryptionPadding.Pkcs1));
        }

         private double[] GetDoubles(byte[] values)
            {
                var result = new double[4];
                Buffer.BlockCopy(values, 0, result, 0, result.Length);
                return result;
            }

    }
}
