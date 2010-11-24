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
        private TcpClient player_client;
        private Socket s;
        private bool connectFlag;
        private byte[] send_buffer;

        public PlayerInteraction(int port)
        {
            connectFlag = false;
            send_buffer = new byte[1000];
            ipAddress = IPAddress.Any;
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine("Listening on port " + port);
            Console.WriteLine("Waiting for Player connections...");

        }

        public void Update(TimeSpan time)
        {
            if(!connectFlag && listener.Pending())
            {
                player_client = listener.AcceptTcpClient();
                Console.WriteLine("Accepted Server Connection");
                s.Blocking = false;
                NetworkStream player_stream = player_client.GetStream();
                ASCIIEncoding test;
                connectFlag = true;
            }
            if (connectFlag && s.Connected)
            {
                Console.WriteLine("Connected");
            }
            if (connectFlag && !s.Connected)
            {
                connectFlag = false;
                Console.WriteLine("Disconnected from Server");
            }

        }

    }
}
