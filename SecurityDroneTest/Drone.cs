using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    /// <summary>
    /// Class for our Drone simulation implementation
    /// </summary>
    public class Drone
    {
        /// <summary>
        /// Drone Psuedo Number generator for our Random number
        /// </summary>
        PRNG Rng { get; set; }

        /// <summary>
        /// Generated client key to be sent to client
        /// </summary>
        byte [] ClientKey { get; set; } 

        /// <summary>
        /// Constructor for our Drone implementation. Creates a PRNG and inits the connection sequence
        /// </summary>
        public Drone()
        {
            Rng = new PRNG();
            SetupConnection();
        }

        /// <summary>
        /// Starts TCP connection, sends setup information and listens for input data.
        /// </summary>
        private void SetupConnection()
        {
            TcpListener drone = null;

            //Get first ipv4 addr 
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            try
            {
                //Start TCP connection
                drone = new TcpListener(ipAddress, 0);
                drone.Start();

                Console.WriteLine("DRONE: Connected on {0}:{1}\n", ((IPEndPoint)drone.LocalEndpoint).Address.ToString(), ((IPEndPoint)drone.LocalEndpoint).Port.ToString());

                //Get connection from client
                TcpClient control = drone.AcceptTcpClient();
                Console.WriteLine("Connected to controller\n");

                //Stream for communicating
                NetworkStream stream = control.GetStream();

                GenerateClientKey();
                //Send clientKey and Seed to controller
                ShareClientKeyAndSeed(stream);

                //need file size to know when to shutdown server
                byte[] bytes = new byte [16];
                stream.Read(bytes);
                long fileSize = BitConverter.ToInt64(bytes);
                Console.WriteLine("file size is {0}", fileSize);

                //start timer
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while(fileSize > 0)
                {
                    //Get data
                    bytes = new byte[32];
                    stream.Read(bytes);
                    fileSize -= 32;
                    HandleCommand(bytes);
                }
                //end timer
                stopwatch.Stop();
                Console.WriteLine("Seconds elapsed: {0} seconds", stopwatch.ElapsedTicks * Math.Pow(10, -7));

                stream.Close();
                control.Close();
                drone.Stop();
            }
            catch(Exception e)
            {
                Console.WriteLine("Oops Drone encountered error" + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Decrypts and prints out command
        /// </summary>
        /// <param name="input">Encrypted data from Controller</param>
        /// <returns>If command is successfully handled</returns>
        private bool HandleCommand(byte [] input)
        {
            byte [] command = EncryptorDecryptor.Decrypt(Rng.getNext(), input, ClientKey);
            double[] decrypt = GetDoubles(command);
            Console.WriteLine("DRONE: Decrypted Command is: {0},{1},{2},{3}", decrypt[0], decrypt[1], decrypt[2], decrypt[3]);
            return true;
        }

        /// <summary>
        /// Generates the ClientKey using a cryptographically secure RNG class.
        /// </summary>
        private void GenerateClientKey()
        {
            //Use new Cryptographic RNG for ClientKey 
            byte[] bytes = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            ClientKey = bytes;
        }

        /// <summary>
        /// Sends ClientKey and seed for PRNG to Client. Use RSA here to ensure security
        /// </summary>
        /// <param name="stream">TCP Connection to Controller</param>
        private void ShareClientKeyAndSeed(NetworkStream stream)
        {
            //Console.WriteLine("Client Key is {0}\n", Encoding.Default.GetString(ClientKey));
            RSA enc = new RSACryptoServiceProvider();
            byte[] cPubKey = new byte[4096];
            //recv client public key
            stream.Read(cPubKey);
            //encrypt the ClientKey with client's public key
            int tmp;
            enc.ImportRSAPublicKey(cPubKey, out tmp);
            stream.Write(enc.Encrypt(ClientKey, RSAEncryptionPadding.Pkcs1));

            //encrypt seed with client's public key
            stream.Write(enc.Encrypt(BitConverter.GetBytes(Rng.Seed.Ticks), RSAEncryptionPadding.Pkcs1));
        }

        /// <summary>
        /// Helper method to convert byte[] to double[]
        /// </summary>
        /// <param name="values">byte[] to be converted</param>
        /// <returns>converted double[]</returns>
        private double[] GetDoubles(byte[] values)
            {
                var result = new double[values.Length / 8];
                Buffer.BlockCopy(values, 0, result, 0, result.Length * 8);
                return result;
            }

    }
}
