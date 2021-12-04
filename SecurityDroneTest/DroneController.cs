using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDroneTest
{
    public class DroneController
    {
        public PRNG Rng { get; set; }

        private byte [] ClientKey { get; set; }

        private DateTime Seed { get; set; }

        public DroneController(string servIP, int port, string filename)
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
            catch(Exception e)
            {
                Console.WriteLine("Ooops, problem with DroneController connection " + e.ToString() + "\n");
            }
        }

        private void GetSetupData(NetworkStream stream)
        {
            try
            {
                //Get Client Key
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
                byte[] bytes = new byte[1024];
                stream.Read(bytes);
                Seed = DateTime.FromBinary(BitConverter.ToInt64(bytes)); 
                //Console.WriteLine("Seed is {0}", Seed);

                //Init Rng
                Rng = new PRNG(Seed);
            }
            catch(Exception e)
            {
                Console.WriteLine("Ooops, problem getting data from Drone" + e.ToString() + "\n");
            }
        }

        private void HandleInput(NetworkStream stream, string filename)
        {
            while(true)
            {
                try
                {
                    //Get user input encrypt and send
                    try
                    {
                        //Will need to make input handle different for actual application
                        using(FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                        {
                            BinaryReader reader = new BinaryReader(fs);
                            byte[] bytes = new byte[32];
                            bytes = reader.ReadBytes(32);

                            byte [] encryptedInput = EncryptorDecryptor.Encrypt(Rng.getNext(), bytes, ClientKey);

                            //Send encrypted data to Drone
                            stream.Write(encryptedInput);
                        }

                    }
                    catch(FormatException e)
                    {
                        //Again will need to rework how we handle input
                        Console.WriteLine("Invalid input: Please input a number\n");
                    }
                }

                catch(Exception e)
                {
                    Console.WriteLine("Ooops, problem sending data to Drone " + e.ToString() + "\n");
                }
            }

        }

    }
}
