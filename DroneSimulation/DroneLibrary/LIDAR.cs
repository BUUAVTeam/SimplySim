using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimplySim.Dynamics;
using SimplySim.Simulation.Engine;
using SimplySim.Events;
using SimplySim.Math;
using PlayerServer;

namespace DroneLibrary
{
    class LIDAR : IActuator
    {
        private IDynamicActor _actor;
        private World _world;
        private string _name;
        private RayDesc desc;
        private RayHit result;
        private Vector3[] rayVec;
        private Vector3 origin;
        private PlayerInteraction _player;
        private Drone _drone;
        private int iter;

        public LIDAR(WorldHandle world, string name, PlayerInteraction Player, Drone drone)
        {
            //Register to actor insertion
            _name = "LIDAR" + Guid.NewGuid().ToString();
            _player = Player;
            world.World.ActorAddedFiltered.Subscribe(new RegexFilter<IActor>("[.]ComplexObject[.]Body"), BindActor);
            world.World.AddActuator(this);
            _world = world.World;
            _drone = drone;
            desc = new RayDesc();
            origin = new Vector3(0, (float)0.1, 0);
            rayVec = new Vector3[1081];
            iter = 0;
            int index = 0;
            for (double i = -45.0; i <= 225; i += 0.25)
            {
                rayVec[index] = new Vector3((float)(Math.Cos(i*(Math.PI/180.0))), 0, (float)(-1.0 * Math.Sin(i*(Math.PI/180.0))));
                index++;
            }
        }

        public void Update(float time)
        {
            try
            {
                if (_actor != null)
                {
                    desc.Origin = _actor.WorldPose.Translation + origin;
                    for (int i = iter; i < iter + 108; i++)
                    {
                        //Origin offset Transform
                        desc.Direction = _actor.WorldPose.Matrix * rayVec[i];
                        //result = _world.RayCastClosest(desc);
                        if (result == null)
                            _player.saveLIDAR(30, i);
                        else
                            _player.saveLIDAR((result.Distance > 30 ? 30 : result.Distance), i);
                    }
                    iter += 108;
                    if (iter > 972)
                    {
                        _player.publishLIDAR();
                        _player.setIMU(_drone.CurrentPitch, _drone.CurrentYaw, _drone.CurrentRoll);
                        _player.setGPS(_drone.GPS);
                        iter = 0;
                    }
                }
            }
            catch (SystemException e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                return;
            }
        }
        private void publish()
        {

            return;

        }

        private void BindActor(Object sender, EventArgs<IActor> args)
        {
            _actor = (IDynamicActor)args.Item;
            
        }
        public string Name
        {
            get { return _name; }
        }


    }
}
