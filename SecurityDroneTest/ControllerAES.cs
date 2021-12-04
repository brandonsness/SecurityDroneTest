using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    class ControllerAES
    {
        AesManaged Enc { get; set; }

        public ControllerAES(string servIP, int port, string filename)
        {
            HandleConnection(servIP, port, filename);
        }

        private void HandleConnection(string servIP, int port, string filename)
        {
            try
            {
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
                Console.WriteLine("Ooops, problem getting data from Drone" + e.ToString() + "\n");
            }
        }

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
                    //Again will need to rework how we handle input
                    Console.WriteLine("Invalid input: Please input a number\n");
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Ooops, problem sending data to Drone " + e.ToString() + "\n");
            }
        }
    }
}
