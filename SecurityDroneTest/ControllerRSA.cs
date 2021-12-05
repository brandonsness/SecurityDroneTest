using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    public class ControllerRSA
    {
        public PRNG Rng { get; set; }

        private byte[] ClientKey { get; set; }

        private DateTime Seed { get; set; }

        public ControllerRSA(string servIP, int port, string filename)
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

                byte[] cPubKey = new byte[4096];
                //recv client public key
                stream.Read(cPubKey);

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
                Console.WriteLine("Ooops, problem with DroneController connection " + e.ToString() + "\n");
            }
        }

        private void HandleInput(NetworkStream stream, string filename, RSA enc)
        {
            try
            {
                //Get user input encrypt and send
                try
                {

                    FileInfo info = new FileInfo(filename);
                    //Console.WriteLine(BitConverter.ToString(enc.ExportRSAPublicKey()));
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
