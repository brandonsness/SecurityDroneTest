using System;
using System.Collections.Generic;
using CommandLine;

namespace SecurityDroneTest
{
    /// <summary>
    /// Main class used to run the different modes
    /// </summary>
    public class Tester
    {
        /// <summary>
        /// Enum for different modes
        /// </summary>
        [Flags]
        public enum ModeEnum
        {
            Drone = 1,
            Controller = 2,
            Data = 3,
            DroneRSA = 4,
            ControllerRSA = 5,
            DroneAES = 6,
            ControllerAES = 7
        };

        /// <summary>
        /// Helper Class used to handle Command line input
        /// </summary>
        public class Options
        {
            [Option('m', "mode", Required = true, HelpText = "Sets mode either Drone, Controller, Data, DroneRSA, ControllerRSA, DroneAES, ControllerAES" )]
            public ModeEnum Mode { get; set; }

            [Option('i', "ip", Required = false, HelpText = "IP of Drone only need if in Controller mode")]
            public string IP { get; set; }

            [Option('p', "port", Required = false, HelpText = "Port of the Drone only need if in Controller mode")]
            public int Port { get; set; }

            [Option('f', "file", Required = false, HelpText = "filename for input file")]
            public string FileName { get; set; }
        }

        /// <summary>
        /// Start of program handles command line argumnts
        /// </summary>
        /// <param name="args">Command line arguments as specified from the Options Class</param>
        public static void Main(string [] args)
        {
            //Parse Command line arguments and start the different modes
            var parser = new Parser();
            var result = parser.ParseArguments<Options>(args).WithParsed(Run)
                .WithNotParsed(Error);
        }

        /// <summary>
        /// Runs the specified modes from the Options
        /// </summary>
        /// <param name="opts">Options specified from command line</param>
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
            else if(opts.Mode == ModeEnum.Data)
            {
                Console.WriteLine("Data entry mode\n");
                DataGenerator.Generate(opts.FileName);
            }
            else if(opts.Mode == ModeEnum.DroneRSA)
            {
                Console.WriteLine("Drone RSA mode\n");
                DroneRSA drone = new DroneRSA();
            }
            else if(opts.Mode == ModeEnum.ControllerRSA)
            {
                Console.WriteLine("Controller RSA mode\n");
                ControllerRSA controller = new ControllerRSA(opts.IP, opts.Port, opts.FileName);
            }
            else if(opts.Mode == ModeEnum.DroneAES)
            {
                Console.WriteLine("Drone AES mode");
                DroneAES Drone = new DroneAES();
            }
            else if(opts.Mode == ModeEnum.ControllerAES)
            {
                Console.WriteLine("Controller AES mode");
                ControllerAES controller = new ControllerAES(opts.IP, opts.Port, opts.FileName);
            }
            else
            {
                Console.WriteLine("Mode not selected");
                Console.WriteLine("USAGE:\n SecurityDroneTest --[m|mode] [Drone|Controller|Data] --[i|ip] [ip address] --[p|port] [port] --[f|file] [filename]\n");
            }
        }

        /// <summary>
        /// Supposed to handle any errors in command line input
        /// </summary>
        /// <param name="err">List of errors</param>
        static void Error(IEnumerable<Error> err)
        {
            Console.WriteLine("Error in usage\n");
            Console.WriteLine("USAGE:\n SecurityDroneTest --[m|mode] [Drone|Controller|Data] --[i|ip] [ip address] --[p|port] [port] --[f|file] [filename]\n");
        }
    }
}
