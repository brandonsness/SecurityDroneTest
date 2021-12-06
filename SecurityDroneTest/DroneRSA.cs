using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace SecurityDroneTest
{
    /// <summary>
    /// RSA implementation of the Drone
    /// </summary>
    public class DroneRSA
    {

        /// <summary>
        /// Constructor for DroneRSA
        /// </summary>
        public DroneRSA()
        {
            SetupConnection();
        }

        /// <summary>
        /// Creates TCP connection and communicates with ControllerRSA
        /// </summary>
        private void SetupConnection()
        {
            TcpListener drone = null;

            //Get first ipv4 addr 
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            try
            {
                //Start connection
                drone = new TcpListener(ipAddress, 0);
                drone.Start();

                Console.WriteLine("DRONE: Connected on {0}:{1}\n", ((IPEndPoint)drone.LocalEndpoint).Address.ToString(), ((IPEndPoint)drone.LocalEndpoint).Port.ToString());

                //Get connection from Drone
                TcpClient control = drone.AcceptTcpClient();
                Console.WriteLine("Connected to controller\n");

                //Stream for communicating
                NetworkStream stream = control.GetStream();

                RSA enc = RSACryptoServiceProvider.Create(4096);
                //send our public key
                stream.Write(enc.ExportRSAPublicKey());

                byte[] bytes = new byte[16];
                stream.Read(bytes);
                long fileSize = BitConverter.ToInt64(bytes);
                Console.WriteLine("file size is {0}", fileSize);

                //start timer
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (fileSize > 0)
                {
                    //Get encrypted data
                    bytes = new byte[512];
                    stream.Read(bytes);
                    fileSize -= 512;
                    HandleCommand(bytes, enc);
                }
                //end timer
                stopwatch.Stop();
                Console.WriteLine("Seconds elapsed: {0} seconds", stopwatch.ElapsedTicks * Math.Pow(10, -7));

                stream.Close();
                control.Close();
                drone.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Oops DroneRSA encountered error" + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Handle Decrypting and executing command
        /// </summary>
        /// <param name="input">Encrypted command data</param>
        /// <param name="enc">RSA class used to decrypt data</param>
        /// <returns>If command was successfully handled</returns>
        private bool HandleCommand(byte[] input, RSA enc)
        {
            byte[] command = enc.Decrypt(input, RSAEncryptionPadding.Pkcs1);
            double[] decrypt = GetDoubles(command);
            //Console.WriteLine("DRONE: Decrypted Command is: {0},{1},{2},{3}", decrypt[0], decrypt[1], decrypt[2], decrypt[3]);
            return true;
        }

        /// <summary>
        /// Helper function to convert from byte[] to double[]
        /// </summary>
        /// <param name="values">byte[] to be converted</param>
        /// <returns>Converted double[]</returns>
        private double[] GetDoubles(byte[] values)
        {
            var result = new double[values.Length / 8];
            Buffer.BlockCopy(values, 0, result, 0, result.Length * 8);
            return result;
        }

    }
}
