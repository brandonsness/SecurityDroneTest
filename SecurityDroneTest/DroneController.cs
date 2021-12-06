using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace SecurityDroneTest
{
    /// <summary>
    /// Our implementation of the Drone Controller
    /// </summary>
    public class DroneController
    {
        /// <summary>
        /// Psuedo Random Number Generator used in our encryption scheme
        /// </summary>
        public PRNG Rng { get; set; }

        /// <summary>
        /// Cryptographically generated Random number used in our encryption scheme
        /// </summary>
        private byte[] ClientKey { get; set; }

        /// <summary>
        /// Seed used to create PRNG
        /// </summary>
        private DateTime Seed { get; set; }

        /// <summary>
        /// Constructor for our Drone Controller
        /// </summary>
        /// <param name="servIP">IP of the Drone connection</param>
        /// <param name="port">Port of the TCP Drone connection</param>
        /// <param name="filename">Filename of the input file for the drone commands</param>
        public DroneController(string servIP, int port, string filename)
        {
            HandleConnection(servIP, port, filename);
        }

        /// <summary>
        /// Starts connection with Drone and sends data
        /// </summary>
        /// <param name="servIP">IP of the Drone connection</param>
        /// <param name="port">Port of the TCP Drone connection</param>
        /// <param name="filename">Filename of the input file for the drone commands</param>
        private void HandleConnection(string servIP, int port, string filename)
        {
            try
            {
                //Connect to Drone
                TcpClient control = new TcpClient(servIP, port);
                NetworkStream stream = control.GetStream();
                Console.WriteLine("Socket connected");

                //get data from server
                GetSetupData(stream);
                Console.WriteLine("Setup complete");

                //Take user input
                HandleInput(stream, filename);

                //Release socket
                stream.Close();
                control.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem with DroneController connection " + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Gets the CLientKey and the Seed from the Drone
        /// </summary>
        /// <param name="stream">TCP Connection to Drone</param>
        private void GetSetupData(NetworkStream stream)
        {
            try
            {
                //Get Client Key
                //Create RSA public and private keys for secure data transfer
                //of just the client key and the seed
                RSA enc = RSACryptoServiceProvider.Create(4096);
                //send our public key
                stream.Write(enc.ExportRSAPublicKey());
                //get encrypted Client Key
                byte[] key = new byte[512];
                stream.Read(key);
                ClientKey = new byte[32];
                ClientKey = enc.Decrypt(key, RSAEncryptionPadding.Pkcs1);
                //Console.WriteLine("Client key is {0}", Encoding.Default.GetString(ClientKey));

                //Get Seed
                byte[] bytes = new byte[512];
                stream.Read(bytes);
                byte[] seed = new byte[32];
                seed = enc.Decrypt(bytes, RSAEncryptionPadding.Pkcs1);
                Seed = DateTime.FromBinary(BitConverter.ToInt64(seed));
                //Console.WriteLine("Seed is {0}", Seed);

                //Init Rng
                Rng = new PRNG(Seed);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem getting data from Drone" + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Gets data from file, encrypts it, and sends it to the Drone
        /// </summary>
        /// <param name="stream">TCP Connection to the Drone</param>
        /// <param name="filename">Filename of the input file</param>
        private void HandleInput(NetworkStream stream, string filename)
        {
            try
            {
                //Get user input encrypt and send
                try
                {
                    FileInfo info = new FileInfo(filename);
                    long fileSize = info.Length;
                    Console.WriteLine("File size {0}", fileSize);
                    stream.Write(BitConverter.GetBytes(fileSize));
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        //start timer
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        //read and send data
                        while (fileSize > 0)
                        {
                            //Get input
                            BinaryReader reader = new BinaryReader(fs);
                            byte[] bytes = new byte[32];
                            bytes = reader.ReadBytes(32);
                            fileSize -= 32;

                            byte[] encryptedInput = EncryptorDecryptor.Encrypt(Rng.getNext(), bytes, ClientKey);

                            //Send encrypted data to Drone
                            stream.Write(encryptedInput);
                        }
                        //end timer
                        stopwatch.Stop();
                        Console.WriteLine("Seconds elapsed: {0} seconds", stopwatch.ElapsedTicks * Math.Pow(10, -7));
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Invalid input Error:\n");
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem sending data to Drone " + e.ToString() + "\n");
            }
        }
    }
}
