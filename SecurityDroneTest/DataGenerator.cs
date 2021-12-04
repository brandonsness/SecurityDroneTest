using System;
using System.IO;

namespace SecurityDroneTest
{
    public static class DataGenerator
    {
        public static void Generate(string fileName)
        {
            double throttle, yaw, pitch, roll;
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            BinaryWriter writer = new BinaryWriter(stream);
            Console.WriteLine("Enter data to generate data file each between -100 and 100");
            while (true)
            {
                while (true)
                {
                    Console.Write("Throttle: ");
                    if (double.TryParse(Console.ReadLine(), out throttle) && throttle >= -100.0 && throttle <= 100.0)
                        break;
                    else
                        Console.WriteLine("input must be a floating point value");
                }
                while (true)
                {
                    Console.Write("\nYaw: ");
                    if (double.TryParse(Console.ReadLine(), out yaw) && yaw >= -100.0 && yaw <= 100.0)
                        break;
                    else
                        Console.WriteLine("input must be a floating point value");
                }
                while (true)
                {
                    Console.Write("\nPitch: ");
                    if (double.TryParse(Console.ReadLine(), out pitch) && pitch >= -100.0 && pitch <= 100.0)
                        break;
                    else
                        Console.WriteLine("input must be a floating point value");
                }
                while (true)
                {
                    Console.Write("\nRoll: ");
                    if (double.TryParse(Console.ReadLine(), out roll) && roll >= -100.0 && roll <= 100.0)
                        break;
                    else
                        Console.WriteLine("input must be a floating point value");
                }
                double[] arr = new double[] { throttle, yaw, pitch, roll };
                byte[] bytes = GetBytes(arr);

                writer.Write(bytes);
                
                Console.WriteLine("Press Enter to continue or type exit to exit");
                string exit = Console.ReadLine();
                if (exit.Equals("exit"))
                {
                    writer.Close();
                    break;
                }
            }

            static byte[] GetBytes(double[] values)
            {
                var result = new byte[values.Length * sizeof(double)];
                Console.WriteLine("result size is {0}", result.Length);
                Buffer.BlockCopy(values, 0, result, 0, result.Length);
                return result;
            }

        }
    }
}
