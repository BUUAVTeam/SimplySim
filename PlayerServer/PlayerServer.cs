using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using maths = SimplySim.Math;

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
        private short[] _lidarData;
        private bool _newLidar;
        private ASCIIEncoding _encoder;
        private float _pitch, _yaw, _roll;
        private maths.Vector3 _pos;

        public PlayerInteraction(int port)
        {
            connectFlag = false;
            _newLidar = false;
            _encoder = new ASCIIEncoding();
            _lidarData = new short[1081];
            send_buffer = new byte[2162];
            ipAddress = IPAddress.Any;
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine("Listening on port " + port);
            Console.WriteLine("Waiting for Player connections...");

        }
        public void setIMU(float pitch, float yaw, float roll)
        {
            _pitch = pitch;
            _yaw = (yaw < 0 ? yaw + 360 : yaw);
            _roll = roll;
            return;
        }
        public void setGPS(maths.Vector3 pos)
        {
            _pos = pos;
            return;
        }
        public void saveLIDAR(float range, int index)
        {
            _lidarData[index] = (short)(range*100.0);
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
                try
                {
                    player_stream.Write(_encoder.GetBytes("LASER"), 0, _encoder.GetByteCount("LASER"));
                    int x = 0;
                    foreach (short f in _lidarData)
                    {
                        //Console.WriteLine("range: " + f.ToString());
                        byte[] temp = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(f));
                        for (int y = 0; y < 2; y++)
                            send_buffer[y + x] = temp[y];
                        x += 2;
                    }
                    player_stream.Write(send_buffer, 0, 2162);

                    //Write IMU & GPS Data
                    player_stream.Write(_encoder.GetBytes("POS3D"), 0, _encoder.GetByteCount("POS3D"));
                    byte[] yaw = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(Math.Abs(_yaw) * (float)1000)));
                    player_stream.Write(yaw, 0, 4);
                    byte[] xpos = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(Math.Abs(_pos.X) * (float)1000)));
                    //if (_pos.X < 0)
                     //   xpos[0] |= 0x80;
          
                    player_stream.Write(xpos, 0, 4);
                    byte[] ypos = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(Math.Abs(_pos.Z) * (float)1000)));
                    player_stream.Write(ypos, 0, 4);

                }
                catch (IOException e)
                {
                    Console.WriteLine("IO Exception: " + e.ToString());
                    connectFlag = false;
                    return;
                }
                _newLidar = false;
            }

        }

    }
}
