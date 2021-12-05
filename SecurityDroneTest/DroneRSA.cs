using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    public class DroneRSA
    {
        PRNG Rng { get; set; }

        byte[] ClientKey { get; set; }

        public DroneRSA()
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
                Console.WriteLine("Oops Drone encountered error" + e.ToString() + "\n");
            }
        }

        private bool HandleCommand(byte[] input, RSA enc)
        {
            byte[] command = enc.Decrypt(input, RSAEncryptionPadding.Pkcs1);
            double[] decrypt = GetDoubles(command);
            //Console.WriteLine("DRONE: Decrypted Command is: {0},{1},{2},{3}", decrypt[0], decrypt[1], decrypt[2], decrypt[3]);
            return true;
        }

        private double[] GetDoubles(byte[] values)
        {
            var result = new double[values.Length / 8];
            Buffer.BlockCopy(values, 0, result, 0, result.Length * 8);
            return result;
        }

    }
}
