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
        private NetworkStream player_stream;
        private bool connectFlag;
        private byte[] send_buffer;
        private float[] _lidarData;
        private bool _newLidar;

        public PlayerInteraction(int port)
        {
            connectFlag = false;
            _newLidar = false;
            _lidarData = new float[1081];
            send_buffer = new byte[4324];
            ipAddress = IPAddress.Any;
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine("Listening on port " + port);
            Console.WriteLine("Waiting for Player connections...");

        }
        public void saveLIDAR(float range, int index)
        {
            _lidarData[index] = range;
            return;
        }
        public void publishLIDAR()
        {
            _newLidar = true;
            return;
        }

        public void Update(TimeSpan time)
        {
            if(!connectFlag && listener.Pending())
            {
                player_client = listener.AcceptTcpClient();
                Console.WriteLine("Accepted Server Connection");
                player_stream = player_client.GetStream();
                connectFlag = true;
            }
            if (connectFlag && player_stream.CanWrite && _newLidar)
            {
                int x = 0;
                foreach(float f in _lidarData)
                {
                    byte[] temp = BitConverter.GetBytes(f);
                    for(int y=0;y<4;y++)
                        send_buffer[y+x]=temp[y];
                    x += 4;
                }
                player_stream.Write(send_buffer,0,4324);
                //publish new lidar data
            }

        }

    }
}
