using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using maths = SimplySim.Math;
using System.Threading;

namespace PlayerServer
{
    public class PlayerInteraction
    {
        private IPAddress ipAddress;
        private TcpListener listener;
        private TcpClient player_client;
        private NetworkStream player_stream;
        private bool connectFlag;
        private short[] _lidarData;
        public bool newLidar, newPos;

        private Thread sendThread;


        private ASCIIEncoding _encoder;
        private maths.Vector3 _pos;
        private bool _contBind;
        public bool controlUpdate;
        public float _deltaZ, _yaw;
        public bool Lidar, Pos, Writing, Publish;
        public maths.Vector3 _pyrD, _pyrIMU;
        public PlayerInteraction(int port)
        {
            connectFlag = false;
            _contBind = false;
            Pos = false;
            Lidar = false;
            Writing = false;
            newLidar = false;
            newPos = false;
            Publish = false;
            _encoder = new ASCIIEncoding();
            _lidarData = new short[1081];
            
            ipAddress = IPAddress.Any;
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine("Listening on port " + port);
            Console.WriteLine("Waiting for Player connections...");

        }

        public void Update(TimeSpan time)
        {
            if(player_stream == null)
                connectFlag = false;
            if(!connectFlag && listener.Pending())
            {
                player_client = listener.AcceptTcpClient();
                Console.WriteLine("Accepted Server Connection");
                player_stream = player_client.GetStream();

                connectFlag = true;
                
                _contBind = true;
            }
            if (connectFlag && player_stream.CanWrite && (newLidar || newPos))
            {
                try
                {
                    if (sendThread == null || !sendThread.IsAlive)
                    {
                        DataRecieve sendData = new DataRecieve(this,player_stream, (newLidar ? _lidarData : null), 
                            (newPos ? _pos : new maths.Vector3(-1000,-1000,-1000)), (newPos ? _yaw : -1000f));
                        sendThread = new Thread(new ThreadStart(sendData.sendData));
                        sendThread.Start();
                        Writing = true;
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("IO Exception: " + e.ToString());
                    connectFlag = false;
                    return;
                }
            }

            if (connectFlag && !player_stream.CanRead)
            {
                connectFlag = false;
            }

            if (connectFlag && player_stream.DataAvailable)
            {
                DataRecieve getData = new DataRecieve(player_stream, this);
                Thread getDataThread = new Thread(new ThreadStart(getData.getData));
                getDataThread.Start();
                
            }
            if (sendThread != null && !sendThread.IsAlive)
            {
                Writing = false;
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

        public maths.Vector3 pyrD { get { return _pyrD; } }

        public float deltaZ { get { return _deltaZ; } }

        public bool contBind { get { return _contBind; } }

    }
}
