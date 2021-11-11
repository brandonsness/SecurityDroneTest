using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace SecurityDroneTest
{
    public class DroneController
    {
        public PRNG Rng { get; set; }
        public DroneController()
        {
            //Create socket
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

    }
}
