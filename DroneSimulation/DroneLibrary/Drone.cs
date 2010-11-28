using System;
using System.Collections.Generic;
using SimplySim.Simulation.Engine;
using SimplySim.Events;
using SimplySim.DataModel;
using SimplySim.Dynamics;
using SimplySim.Xml;
using SimplySim.IO;
using PlayerServer;

using maths = SimplySim.Math;
using SimplySim.Xna.Engine;

namespace DroneLibrary
{
    public class Drone
    {
        private IDynamicActor _body;
        private LIDAR _LIDAR;
        private Dictionary<string, Rotor> _dictionaryRotors;
        private PlayerInteraction _player;

        public Drone(WorldHandle world, string name, PlayerInteraction Player)
        {
            DroneConfig config = Serializer.Instance.Deserialize<DroneConfig>(new Path(name + ".drs"));
            _player = Player;

            _dictionaryRotors = new Dictionary<string, Rotor>();

            foreach (RotorDesc rotorSpecified in config.Rotors)
            {
                Rotor r = new Rotor(world.World, rotorSpecified, name);
                _dictionaryRotors.Add(rotorSpecified.Name, r);
            }
            _LIDAR = new LIDAR(world, name,_player, this);

            world.World.ActorAddedFiltered.Subscribe(new RegexFilter<IActor>(name + "[.]ComplexObject[.]" + config.BodyName), BindActor);
        }

        public Dictionary<string, Rotor> Rotors
        {
            get { return _dictionaryRotors; }
        }

        public bool IsInitialized
        {
            get
            {
                bool initialized = true;
                foreach (Rotor rotor in _dictionaryRotors.Values)
                {
                    if (!rotor.IsInitialized)
                    {
                        initialized = false;
                        break;
                    }
                }
                return _body != null && initialized;
            }
        }

        public float CurrentYaw
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Drone is not initialized !");
                }
                return ((float)Math.Atan2(_body.WorldPose.Matrix.M31, _body.WorldPose.Matrix.M11) * 180 / (float)Math.PI);
            }
        }

        public float CurrentRoll
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Drone is not initialized !");
                }
                return ((float)Math.Atan2(_body.WorldPose.Matrix.M21, _body.WorldPose.Matrix.M22) * 180 / (float)Math.PI);
            }
        }

        public float CurrentPitch
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Drone is not initialized !");
                }
                return ((float)Math.Atan2(_body.WorldPose.Matrix.M23, _body.WorldPose.Matrix.M22) * 180 / (float)Math.PI);
            }
        }

        public maths.Matrix33 Pose
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Drone is not initialized !");
                }
                return _body.WorldPose.Matrix;
            }
        }

        public maths.Vector3 GPS
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Drone is not initialized !");
                }
                return _body.WorldPose.Translation;
            }
        }

        private void BindActor(Object sender, EventArgs<IActor> args)
        {
            _body = (IDynamicActor)args.Item;
           // _LIDAR.BindActor(sender, args);
        }
    }
}
