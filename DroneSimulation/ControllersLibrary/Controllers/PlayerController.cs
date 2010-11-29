using System;
using System.Collections.Generic;
using DroneLibrary;
using Microsoft.Xna.Framework.Graphics;
using SimplySim.Xna.Engine;
using Microsoft.Xna.Framework;
using ControllersLibrary.Controllers;
using PlayerServer;
using maths = SimplySim.Math;

namespace ControllersLibrary
{
    public class PlayerController : SimplePIDController
    {
        private bool _isStarted;
        private Drone _drone;
        private PlayerInteraction _player;
        private double _deltaZ, _deltaYaw;
        private AbstractDroneCommand _command;

        public PlayerController(PlayerInteraction player, Drone drone, float mass, float gravity, AbstractDroneCommand droneCommand, Parameter[] parameters) 
            : base (drone, mass, gravity, droneCommand, parameters)
        {
            _command = droneCommand;
            _player = player;
            _deltaZ = 0;
            _drone = drone;
            _isStarted = false;
            return;
        }

        public new void Update(float time)
        {
            if (_drone.IsInitialized)
            {
                if (_player.contBind)
                {
                    if (_player.controlUpdate)
                    {
                        maths.Vector3 pyrD = _player.pyrD;
                        base.TargetPitch = (float)(180.0 / Math.PI) * pyrD.X;
                        _deltaYaw = (double)pyrD.Y;
                        base.TargetRoll = (float)(180.0 / Math.PI) * pyrD.Z;
                        _deltaZ = (double)_player.deltaZ;
                        _player.controlUpdate = false;

                    }
                    updateAltitude(time);
                    
                }
            }
            base.Update(time);
        }

        private void updateAltitude(float time)
        {
            base.TargetAltitude = Math.Min(Math.Max(base.TargetAltitude+(float)_deltaZ * time,0),5);
            base.TargetYaw = base.TargetYaw + (time * (float)_deltaYaw * (float)(180.0 / Math.PI));
            return;
        }

    }
}
