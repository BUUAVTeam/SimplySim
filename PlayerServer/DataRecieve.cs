using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using SimplySim.Math;

namespace PlayerServer
{
    class DataRecieve
    {
        private NetworkStream _datapath;
        private PlayerInteraction _parentHandle;
        private ASCIIEncoding _encoder;
        private Vector3 _pos;
        private float _yaw;
        private short[] _lidarData;
        private byte[] send_buffer = new byte[2162];

        public DataRecieve(NetworkStream stream, PlayerInteraction parentHandle)
        {
            _encoder = new ASCIIEncoding();
            _datapath = stream;
            _parentHandle = parentHandle;
            return;
        }
        public DataRecieve(NetworkStream stream, short[] lidarData, Vector3 pos, float yaw)
        {
            _encoder = new ASCIIEncoding();
            _datapath = stream;
            _lidarData = lidarData;
            _pos = pos;
            _yaw = yaw;
            return;
        }
        public void getData()
        {
            try
            {
                if (!_datapath.CanRead || !_datapath.DataAvailable)
                    return;
                int _recvd;
                byte[] recv_buffer = new byte[256];
                _recvd = _datapath.Read(recv_buffer, 0, 256);
                string msg = _encoder.GetString(recv_buffer);
                if (msgCompare(msg,"VELC"))
                {
                    //Console.WriteLine("GOT START TAG");//receive vel command data;
                    //recvd = player_stream.Read(recv_buffer, 0, 16);
                    _parentHandle._pyrD.X = (float)(BitConverter.ToInt32(recv_buffer, 4) / 1000.0); //Pitch
                    _parentHandle._pyrD.Y = -(float)(BitConverter.ToInt32(recv_buffer, 8) / 1000.0); //Yaw
                    _parentHandle._pyrD.Z = (float)(BitConverter.ToInt32(recv_buffer, 12) / 1000.0); //Roll
                    _parentHandle._deltaZ = (float)(BitConverter.ToInt32(recv_buffer, 16) / 1000.0); //DeltaZ
                    _parentHandle.controlUpdate = true;
                }
                else if (msgCompare(msg,"LASS"))
                {
                    _parentHandle.Lidar = true;
                    return;
                }
                else if (msgCompare(msg,"LAUS"))
                {
                    _parentHandle.Lidar = false;
                }
                else if (msgCompare(msg,"POSS"))
                {
                    _parentHandle.Pos = true;
                }
                else if (msgCompare(msg, "POUS"))
                {
                    _parentHandle.Pos = false;
                }
                else
                {
                    Console.WriteLine("Unknown 00 Message: " + " Len: " + _recvd.ToString() + " ");
                }
            }
            catch (SocketException e)
            {
                Console.Error.WriteLine("Lost Connection with Player: " + e.ToString());
                _datapath.Dispose();
                _datapath = null;
            }
            return;


        }
        private bool msgCompare(string msg, string cmp)
        {
            if (msg.Substring(0, 4).CompareTo(cmp) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void sendData()
        {
            try
            {
                if (_lidarData != null)
                {
                    _datapath.Write(_encoder.GetBytes("LASER"), 0, _encoder.GetByteCount("LASER"));
                    int x = 0;
                    foreach (short f in _lidarData)
                    {
                        //Console.WriteLine("range: " + f.ToString());
                        byte[] temp = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(f));
                        for (int y = 0; y < 2; y++)
                            send_buffer[y + x] = temp[y];
                        x += 2;
                    }
                    _datapath.Write(send_buffer, 0, 2162);
                }
                if (_yaw != -1000f)
                {
                    //Write IMU & GPS Data
                    _datapath.Write(_encoder.GetBytes("POS3D"), 0, _encoder.GetByteCount("POS3D"));
                    byte[] yaw = BitConverter.GetBytes((int)(_yaw * 1000f));
                    _datapath.Write(yaw, 0, 4);
                    byte[] xpos = BitConverter.GetBytes((int)(_pos.X * 1000f));
                    _datapath.Write(xpos, 0, 4);
                    byte[] ypos = BitConverter.GetBytes((int)(_pos.Z * 1000f));
                    _datapath.Write(ypos, 0, 4);
                }

            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Lost Connection with Player Server: " + e.ToString());
                _datapath.Dispose();
                _datapath = null;
            }
            return;
        }
    }
}
