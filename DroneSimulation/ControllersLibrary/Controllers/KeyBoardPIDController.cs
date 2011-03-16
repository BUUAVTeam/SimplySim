using System;
using Microsoft.Xna.Framework.Input;
using DroneLibrary;
using ControllersLibrary.Controllers;

namespace ControllersLibrary
{
    public class KeyBoardPIDController : GRASPController
    {
        private const float NTargetPitch = 2.92f;
        private const float NTargetYaw = 0f;
        private const float NTargetRoll = 2.92f;
        private const float NTargetZRate = 1f;

        private float TIMER;
        private float dX, dY, dZ, dXs, dYs, dZs;
        private bool TimerStart;

        private Drone _drone;


        private KeyboardState _previousState;
        private bool _isStarted, _rotorsEnabled;

        public KeyBoardPIDController(Drone drone, float mass, float gravity, AbstractDroneCommand droneCommand, Parameter[] parameters)
            : base(drone, mass, gravity, droneCommand, parameters)
        {
            _isStarted = true;
            _rotorsEnabled = true;
            TIMER = 0;
            TimerStart = false;
            _drone = drone;
            droneCommand.motorStart = true;
            dX = 0f;
            dY = 0f;
            dZ = 0f;
        }

        public void Update(KeyboardState state, bool isControlAllowed, float timeStep)
        {
            if (_isStarted && _rotorsEnabled)
            {
                if (isControlAllowed)
                {
                    if (state.IsKeyDown(Keys.D))
                    {
                        base.TargetRoll = -NTargetRoll;
                    }
                    else if (state.IsKeyDown(Keys.A))
                    {
                        base.TargetRoll = NTargetRoll;
                    }
                    else
                    {
                        base.TargetRoll = 0f;
                    }

                    if (state.IsKeyDown(Keys.W))
                    {
                        base.TargetPitch = NTargetPitch;
                        if (!TimerStart)
                        {
                            TimerStart = true;
                            dXs = _drone.GPS.X;
                            dYs = _drone.GPS.Y;
                            dZs = _drone.GPS.Z;
                        }
                    }
                    else if (state.IsKeyDown(Keys.S))
                    {
                        base.TargetPitch = -NTargetPitch;
                    }
                    else
                    {
                        base.TargetPitch = 0f;
                    }

                    if (state.IsKeyDown(Keys.Up))
                    {
                        base.TargetZAccel = NTargetZRate;
                    }
                    else if (state.IsKeyDown(Keys.Down))
                    {
                        base.TargetZAccel = -NTargetZRate;
                    }
                    else
                    {
                        base.TargetZAccel = 0f;
                    }
                    if (TimerStart)
                        TIMER += timeStep;
                    if (TimerStart && _drone.CurrentPitch > 2.92f)
                    {
                        TimerStart = false;
                        dX = _drone.GPS.X - dXs;
                        dY = _drone.GPS.Y - dYs;
                        dZ = _drone.GPS.Z - dZs;
                    }
                }
                base.Update(timeStep);
            }
            else if (_isStarted)
            {
                base.StopDroneEngines();
                _isStarted = false;
            }
            else if (_rotorsEnabled)
            {
                base.StartDroneEngines();
                _isStarted = true;
            }

            if (isControlAllowed)
            {
                if (state.IsKeyUp(Keys.E) && _previousState.IsKeyDown(Keys.E))
                {
                    //_rotorsEnabled = !_rotorsEnabled;
                }
            }

            _previousState = state;
        }
    }
}
