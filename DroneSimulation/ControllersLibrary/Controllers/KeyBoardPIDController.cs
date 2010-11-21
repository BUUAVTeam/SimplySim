using System;
using Microsoft.Xna.Framework.Input;
using DroneLibrary;
using ControllersLibrary.Controllers;

namespace ControllersLibrary
{
    public class KeyBoardPIDController : SimplePIDController
    {
        private const int MaxTargetPitch = 30;
        private const int MaxTargetRoll = 30;
        private const float StepTargetPitch = 0.5f;
        private const float StepTargetYaw = 0.5f;
        private const float StepTargetRoll = 0.5f;
        private const float StepTargetAltitude = 0.1f;

        private KeyboardState _previousState;
        private bool _isStarted, _rotorsEnabled;

        public KeyBoardPIDController(Drone drone, float mass, float gravity, AbstractDroneCommand droneCommand, Parameter[] parameters)
            : base(drone, mass, gravity, droneCommand, parameters)
        {
            _isStarted = false;
            _rotorsEnabled = false;
        }

        public void Update(KeyboardState state, bool isControlAllowed, float timeStep)
        {
            if (_isStarted && _rotorsEnabled)
            {
                if (isControlAllowed)
                {
                    if (state.IsKeyDown(Keys.B))
                    {
                        base.TargetYaw += StepTargetYaw;
                    }
                    else if (state.IsKeyDown(Keys.V))
                    {
                        base.TargetYaw -= StepTargetYaw;
                    }

                    if (state.IsKeyDown(Keys.Up))
                    {
                        base.TargetPitch = Math.Min(MaxTargetPitch, base.TargetPitch + StepTargetPitch);
                    }
                    else if (state.IsKeyDown(Keys.Down))
                    {
                        base.TargetPitch = Math.Max(-MaxTargetPitch, base.TargetPitch - StepTargetPitch);
                    }
                    else
                    {
                        base.TargetPitch = Math.Min(Math.Max(base.TargetPitch - StepTargetPitch, 0), base.TargetPitch + StepTargetPitch);
                    }

                    if (state.IsKeyDown(Keys.Left))
                    {
                        base.TargetRoll = Math.Min(MaxTargetRoll, base.TargetRoll + StepTargetRoll);
                    }
                    else if (state.IsKeyDown(Keys.Right))
                    {
                        base.TargetRoll = Math.Max(-MaxTargetRoll, base.TargetRoll - StepTargetRoll);
                    }
                    else
                    {
                        base.TargetRoll = Math.Min(Math.Max(base.TargetRoll - StepTargetRoll, 0), base.TargetRoll + StepTargetRoll);
                    }

                    if (state.IsKeyDown(Keys.Q))
                    {
                        base.TargetAltitude += StepTargetAltitude;
                    }
                    else if (state.IsKeyDown(Keys.W))
                    {
                        base.TargetAltitude -= StepTargetAltitude;
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
                    _rotorsEnabled = !_rotorsEnabled;
                }
            }

            _previousState = state;
        }
    }
}
