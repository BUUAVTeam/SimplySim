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
        private int curr;

        public LIDAR(WorldHandle world, string name, PlayerInteraction Player)
        {
            //Register to actor insertion
            _name = "LIDAR" + Guid.NewGuid().ToString();
            _player = Player;
            world.World.ActorAddedFiltered.Subscribe(new RegexFilter<IActor>("[.]ComplexObject[.]Body"), BindActor);
            world.World.AddActuator(this);
            _world = world.World;
            desc = new RayDesc();
            origin = new Vector3(0, 1, 0);
            rayVec = new Vector3[1081];
            curr = -1;
            int index = 0;
            for (double i = -45.0; i <= 225; i += 0.25)
            {
                rayVec[index] = new Vector3((float)(Math.Cos(i)), 0, (float)(-1.0 * Math.Sin(i)));
                index++;
            }
        }

        public void Update(float time)
        {
            try
            {
                if (_actor != null)
                {
                    int thisIt = (int)(time * 10810.0);
                    int i;
                    for (i = 1; i <= thisIt; i++)
                    {
                        if ((curr + i) >= rayVec.Length)
                        {
                            curr = -1;
                            _player.publishLIDAR();
                            return;
                        }
                        desc.Origin = _actor.WorldPose * origin; //Origin offset Transform
                        desc.Direction = _actor.WorldPose.Matrix * rayVec[i+curr];
                        result = _world.RayCastClosest(desc);
                        if (result != null)
                            _player.saveLIDAR((result.Distance > 30 ? 30 : result.Distance),(curr+i));
                    }
                    curr += i;
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
