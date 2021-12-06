using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace SecurityDroneTest
{
    /// <summary>
    /// Class for the RSA version of the Drone Controller
    /// </summary>
    public class ControllerRSA
    {

        /// <summary>
        /// Constructor for the RSA Drone Controller
        /// </summary>
        /// <param name="servIP">IP of the DroneRSA</param>
        /// <param name="port">Port number of the DroneRSA TCP connection</param>
        /// <param name="filename">Filename of the input data file</param>
        public ControllerRSA(string servIP, int port, string filename)
        {
            HandleConnection(servIP, port, filename);
        }

        /// <summary>
        /// Handles connection to the RSA Drone
        /// </summary>
        /// <param name="servIP">IP of the DroneRSA</param>
        /// <param name="port">Port number of the DroneRSA TCP connection</param>
        /// <param name="filename">Filename of the input data file</param>
        private void HandleConnection(string servIP, int port, string filename)
        {
            try
            {
                //Get TCP connection to drone
                TcpClient control = new TcpClient(servIP, port);
                NetworkStream stream = control.GetStream();

                Console.WriteLine("Socket connected");

                byte[] cPubKey = new byte[4096];
                //recv public key of DroneRSA
                stream.Read(cPubKey);

                //set public key to Drone's public key for encrypting
                RSA enc = new RSACryptoServiceProvider();
                int tmp;
                enc.ImportRSAPublicKey(cPubKey, out tmp);

                //Take user input
                HandleInput(stream, filename, enc);

                //Release socket
                stream.Close();
                control.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem with ControllerRSA connection " + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Grabs, encrypts, and sends data to the RSA Drone
        /// </summary>
        /// <param name="stream">TCP Connection to the RSA Drone</param>
        /// <param name="filename">filename of Input file</param>
        /// <param name="enc">RSA encryptor for encrypting data</param>
        private void HandleInput(NetworkStream stream, string filename, RSA enc)
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
                            //Get data
                            BinaryReader reader = new BinaryReader(fs);
                            byte[] bytes = new byte[32];
                            bytes = reader.ReadBytes(32);
                            fileSize -= 32;

                            //RSA encryption
                            byte[] encryptedInput = enc.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);

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
                    Console.WriteLine("Invalid input Error");
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem sending data to DroneRSA " + e.ToString() + "\n");
            }
        }
    }
}
