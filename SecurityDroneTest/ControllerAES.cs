using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    /// <summary>
    /// ControllerAES is the class for the AES version of the drone controller
    /// It handles gathering, encrypting, and sending data to the AES version of the drone
    /// </summary>
    class ControllerAES
    {
        /// <summary>
        /// AES class used for encrypting data
        /// </summary>
        AesManaged Enc { get; set; }

        /// <summary>
        /// Constructor for the ControllerAES class
        /// </summary>
        /// <param name="servIP">IP of the Simulated Drone</param>
        /// <param name="port">Port number the Drone connection is bound to</param>
        /// <param name="filename">Filename of the input file used to simulate drone commands</param>
        public ControllerAES(string servIP, int port, string filename)
        {
            HandleConnection(servIP, port, filename);
        }

        /// <summary>
        /// Function that handles the connection for the controller
        /// </summary>
        /// <param name="servIP">IP of the Simulated Drone</param>
        /// <param name="port">Port number the Drone connection is bound to</param>
        /// <param name="filename">Filename of the input file used to simulate drone commands</param>
        private void HandleConnection(string servIP, int port, string filename)
        {
            try
            {
                //Set up TCP socket connection to drone
                TcpClient control = new TcpClient(servIP, port);
                NetworkStream stream = control.GetStream();

                Console.WriteLine("Socket connected");

                //get data from server
                GetSetupData(stream);
                Console.WriteLine("Setup complete");

                //Grab and send input data
                HandleInput(stream, filename);

                //Release socket
                stream.Close();
                control.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem with ControllerAES connection " + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Creates AES key and sends data to connected DroneAES
        /// </summary>
        /// <param name="stream">Connection to the DroneAES</param>
        private void GetSetupData(NetworkStream stream)
        {
            try
            {
                //Create AES key
                Enc = new AesManaged();
                Enc.Padding = PaddingMode.Zeros; 
                Enc.Mode = CipherMode.CBC;
                Console.WriteLine("key is {0} iv is {1} ", Encoding.Default.GetString(Enc.Key), Encoding.Default.GetString(Enc.IV));
                //As noted in DroneAES.cs this is not secure
                //but acceptable for this simulation
                stream.Write(Enc.Key);
                stream.Write(Enc.IV);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem getting data from DroneAES" + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Grabs the data from the input file, encrypts it, and sends it to the connected DroneAES
        /// </summary>
        /// <param name="stream">Connection to the DroneAES</param>
        /// <param name="filename">FileName that holds input data</param>
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
                        var encryptor = Enc.CreateEncryptor(Enc.Key, Enc.IV);
                        //start timer
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        //read and send data
                        while (fileSize > 0)
                        {
                            //grab data
                            BinaryReader reader = new BinaryReader(fs);
                            byte[] bytes = new byte[32];
                            bytes = reader.ReadBytes(32);
                            fileSize -= 32;

                            //encrypt data
                            byte[] encryptedInput;

                            using MemoryStream ms = new MemoryStream();
                            using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                            using BinaryWriter writer = new BinaryWriter(cs);

                            writer.Write(bytes);
                            cs.FlushFinalBlock();
                            encryptedInput = ms.ToArray();

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
                Console.WriteLine("Ooops, problem sending data to DroneAES " + e.ToString() + "\n");
            }
        }
    }
}
