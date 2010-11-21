using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PlayerServer
{
    public class PlayerInteraction
    {
        private IPAddress ipAddress;
        private TcpListener listener;
        private Socket s;

        public PlayerInteraction(int port)
        {
            ipAddress = IPAddress.Any;
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine("Listening on port " + port);
            Console.WriteLine("Waiting for Player connections...");

        }

        public void Update(TimeSpan time)
        {
            if(listener.Pending() && s != null && !s.Connected)
            {
                s = listener.AcceptSocket();
                Console.WriteLine("Accepted Server Connection");
                s.Blocking = false;
            }
            if (s != null && s.Connected)
            {
                Console.WriteLine("Connected");

            }

        }

    }
}
