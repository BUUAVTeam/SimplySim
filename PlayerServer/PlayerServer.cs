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

        private int _recvd;

        private ASCIIEncoding _encoder;
        private maths.Vector3 _pos;
        private bool _contBind;
        public bool controlUpdate;
        private float _deltaZ, _yaw;
        private maths.Vector3 _pyrD, _pyrIMU;

        public PlayerInteraction(int port)
        {
            connectFlag = false;
            _contBind = false;
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

        public void Update(TimeSpan time)
        {
            if(!connectFlag && listener.Pending())
            {
                player_client = listener.AcceptTcpClient();
                Console.WriteLine("Accepted Server Connection");
                player_stream = player_client.GetStream();

                connectFlag = true;
                
                _contBind = true;
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

            if (connectFlag && player_stream.DataAvailable)
            {
                byte[] recv_buffer = new byte[256];
                if ((_recvd = player_stream.Read(recv_buffer, 0, 256)) < 20)
                    throw new SocketException();
                string msg = _encoder.GetString(recv_buffer);
                if (msg.CompareTo("VELC") == 0)
                {
                    //Console.WriteLine("GOT START TAG");//receive vel command data;
                    //recvd = player_stream.Read(recv_buffer, 0, 16);
                    _pyrD.X = (float)(BitConverter.ToInt32(recv_buffer, 4) / 1000.0); //Pitch
                    _pyrD.Y = -(float)(BitConverter.ToInt32(recv_buffer, 8) / 1000.0); //Yaw
                    _pyrD.Z = (float)(BitConverter.ToInt32(recv_buffer, 12) / 1000.0); //Roll
                    _deltaZ = (float)(BitConverter.ToInt32(recv_buffer, 16) / 1000.0); //DeltaZ
                    controlUpdate = true;
                }
                else
                {
                    Console.WriteLine("Unknown Message: " + msg + " ");
                }
            }     
        }

        public void setIMU(float pitch, float yaw, float roll)
        {
            _pyrIMU.X = pitch;
            _pyrIMU.Y = (yaw < 0 ? yaw + 360 : yaw);
            _pyrIMU.Z = roll;
            return;
        }

        public void setGPS(maths.Vector3 pos)
        {
            _pos = pos;
            return;
        }

        public void saveLIDAR(float range, int index)
        {
            _lidarData[index] = (short)(range * 100.0);
            return;
        }

        public void publishLIDAR()
        {
            _newLidar = true;
            return;
        }

        public maths.Vector3 pyrD { get { return _pyrD; } }

        public float deltaZ { get { return _deltaZ; } }

        public bool contBind { get { return _contBind; } }

    }
}
