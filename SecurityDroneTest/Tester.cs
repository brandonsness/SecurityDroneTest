using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace SecurityDroneTest
{
    public class Tester
    {
        [Flags]
        public enum ModeEnum
        {
            Drone = 1,
            Controller = 2
        };

        public class Options
        {
            [Option('m', "mode", Required = true, HelpText = "Sets mode either Drone or Controller" )]
            public ModeEnum Mode { get; set; }

            [Option('i', "ip", Required = false, HelpText = "IP of Drone only need if in Controller mode")]
            public string IP { get; set; }

            [Option('p', "port", Required = false, HelpText = "Port of the Drone only need if in Controller mode")]
            public int Port { get; set; }

            [Option('f', "file", Required = false, HelpText = "filename for input file")]
            public string FileName { get; set; }
        }

        public static void Main(string [] args)
        {
            /*
            // Demonstrating public/private key generation. Can delete later.
            DroneRSA keys = new DroneRSA();
            Console.WriteLine(BitConverter.ToString(keys.rsaKeyInfo.Exponent));
            Console.WriteLine(BitConverter.ToString(keys.rsaKeyInfo.D));

            Drone drone = new Drone();
            //DroneController control = new DroneController();
            */
            var parser = new Parser();
            var result = parser.ParseArguments<Options>(args).WithParsed(Run)
                .WithNotParsed(Error);
        }

        static void Run(Options opts)
        {
            if(opts.Mode == ModeEnum.Drone)
            {
                Console.WriteLine("Drone mode\n");
                Drone drone = new Drone();
            }
            else if(opts.Mode == ModeEnum.Controller)
            {
                Console.WriteLine("Controller mode\n");
                DroneController controller = new DroneController(opts.IP, opts.Port, opts.FileName);
            }
            else
            {
                Console.WriteLine("Mode not selected");
                Console.WriteLine("USAGE:\n SecurityDroneTest --[m|mode] [Drone|Controller] --[i|ip] [ip address] --[p|port] [port] --[f|file] [filename]\n");
            }
        }

        static void Error(IEnumerable<Error> err)
        {
            Console.WriteLine("Error in usage\n");
            Console.WriteLine("USAGE:\n SecurityDroneTest --[m|mode] [Drone|Controller] --[i|ip] [ip address] --[p|port] [port] --[f|file] [filename]\n");
        }
    }
}
