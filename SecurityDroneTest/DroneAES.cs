﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    /// <summary>
    /// AES implementation of the Drone
    /// </summary>
    class DroneAES
    {

        /// <summary>
        /// Constructor for the AES version of the drone
        /// </summary>
        public DroneAES()
        {
            SetupConnection();
        }

        /// <summary>
        /// Sets up the TCP Connection, Creates AES key and communicates with Controller
        /// </summary>
        private void SetupConnection()
        {
            TcpListener drone = null;

            //Get first ipv4 addr 
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            try
            {
                //start connection
                drone = new TcpListener(ipAddress, 0);
                drone.Start();

                Console.WriteLine("DRONE: Connected on {0}:{1}\n", ((IPEndPoint)drone.LocalEndpoint).Address.ToString(), ((IPEndPoint)drone.LocalEndpoint).Port.ToString());

                //Gets connection to ControllerAES
                TcpClient control = drone.AcceptTcpClient();
                Console.WriteLine("Connected to controller\n");

                //Stream for communicating
                NetworkStream stream = control.GetStream();

                //init AES
                byte[] key = new byte[32];
                byte[] iv = new byte[16];
                // Note: this is not secure. We're sending a symmetric key over a socket
                // anyone listening would get our key. For this simulation we're assuming that
                // the drone and controller would have some other way of sharing the key.
                // Since we just want data on how long the encrytpion and decryption takes this is acceptable here.
                stream.Read(key);
                stream.Read(iv);
                Console.WriteLine("key is {0} iv is {1} ", Encoding.Default.GetString(key), Encoding.Default.GetString(iv));
                AesManaged enc = new AesManaged();
                enc.Padding = PaddingMode.Zeros;
                enc.Mode = CipherMode.CBC;
                var decryptor = enc.CreateDecryptor(key, iv);

                //Filesize important to know so we can shutdown Drone
                byte[] bytes = new byte [16];
                stream.Read(bytes);
                long fileSize = BitConverter.ToInt64(bytes);
                Console.WriteLine("file size is {0}", fileSize);

                //start timer
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while(fileSize > 0)
                {
                    byte[] input = new byte[32];
                    string plaintext;
                    stream.Read(input);
                    //decrypt
                    using MemoryStream ms = new MemoryStream(input);
                    using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                    using StreamReader reader = new StreamReader(cs);
                    
                    plaintext = reader.ReadToEnd();
                    
                    fileSize -= 32;
                    HandleCommand(Encoding.Default.GetBytes(plaintext));
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
                Console.WriteLine("Oops DroneAES encountered error" + e.ToString() + "\n");
            }
        }

        /// <summary>
        /// Handles the Command
        /// </summary>
        /// <param name="input">Decrypted byte[] of the command</param>
        /// <returns>If Command was correctly handled</returns>
        private bool HandleCommand(byte [] input)
        {
            double[] decrypt = GetDoubles(input);
            Console.WriteLine("DRONE: Decrypted Command is: {0},{1},{2},{3}", decrypt[0], decrypt[1], decrypt[2], decrypt[3]);
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
