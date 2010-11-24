﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimplySim.Dynamics;
using SimplySim.Simulation.Engine;
using SimplySim.Events;
using SimplySim.Math;

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
        private float timeInt;
        private float[] lidar_results;

        public LIDAR(WorldHandle world, string name)
        {
            //Register to actor insertion
            _name = "LIDAR" + Guid.NewGuid().ToString();
            world.World.ActorAddedFiltered.Subscribe(new RegexFilter<IActor>("[.]ComplexObject[.]Body"), BindActor);
            world.World.AddActuator(this);
            _world = world.World;
            desc = new RayDesc();
            timeInt = 0;
            origin = new Vector3(0, 1, 0);
            rayVec = new Vector3[1081];
            lidar_results = new float[1081];
            int index = 0;
            for (double i = -45.0; i <= 225; i += 0.25)
            {
                rayVec[index] = new Vector3((float)(Math.Cos(i)), 0, (float)(-1.0 * Math.Sin(i)));
                index++;
            }
        }

        public void Update(float time)
        {
            Console.WriteLine("Time Interval: " + time.ToString());
            
            try{
                if (_actor != null)
                {
                    for (int i = 0; i < rayVec.Length; i++)
                    {
                        desc.Origin = _actor.WorldPose * origin; //Origin offset Transform
                        desc.Direction = _actor.WorldPose.Matrix * rayVec[i];
                        result = _world.RayCastClosest(desc);
                        if (result != null)
                            lidar_results[i] = (result.Distance > 30 ? 30 : result.Distance);
                    }
                }
            }catch(SystemException e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                return;
            }

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
