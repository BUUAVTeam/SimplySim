using System;
using System.Collections.Generic;
using DroneLibrary;
using Microsoft.Xna.Framework.Graphics;
using SimplySim.Xna.Engine;
using Microsoft.Xna.Framework;
using ControllersLibrary.Controllers;

namespace ControllersLibrary
{
    public class GRASPController
    {
        private float _previousYawError, _previousPitchError, _previousRollError;
        private Drone _drone;
        private float _droneMass;
        private float _gravity;
        private float _omegaHover;
        private float _deltaOmegaPhi, _deltaOmegaPsi, _deltaOmegaTheta, _deltaOmegaF;
        private float _omegaMotorOne, _omegaMotorTwo, _omegaMotorThree, _omegaMotorFour;

        private const float _RadToRPM = 60f / (2f * (float)Math.PI);

        private AbstractDroneCommand _droneCommand;

        private Parameter _phiParameter, _thetaParameter, _psiParameter, _zParameter;

        public GRASPController(Drone drone, float mass, float gravity, AbstractDroneCommand droneCommand, Parameter[] parameters)
        {
            _drone = drone;
            _droneMass = mass;
            _gravity = gravity;
            _droneCommand = droneCommand;

            _previousYawError = 0f;
            _previousPitchError = 0f;
            _previousRollError = 0f;


            _phiParameter = new Parameter(ParameterType.Phi, new Coefficients(0, 0, 0));
            _thetaParameter = new Parameter(ParameterType.Theta, new Coefficients(0, 0, 0));
            _psiParameter = new Parameter(ParameterType.Psi, new Coefficients(0, 0, 0));
            _zParameter = new Parameter(ParameterType.z, new Coefficients(0, 0, 0));

            foreach (Parameter param in parameters)
            {
                switch (param.Type)
                {
                    case ParameterType.Phi:
                        _phiParameter = param;
                        break;

                    case ParameterType.Theta:
                        _thetaParameter = param;
                        break;

                    case ParameterType.Psi:
                        _psiParameter = param;
                        break;
                    case ParameterType.z:
                        _zParameter = param;
                        break;
                }
            }
            //calculate omegaHover

            _omegaHover =  (float)Math.Sqrt((double)(_droneMass * _gravity) / (4 * _zParameter.Coefficients.Kp));
        }

        protected Drone Drone { get { return _drone; } }

        public void Update(float timeStep)
        {
            if (_drone.IsInitialized && _droneCommand.motorStart && (timeStep>0))
            {
                AltitudeControl(timeStep);
                YawControl(timeStep);
                PitchControl(timeStep);
                RollControl(timeStep);

                _omegaMotorOne = (_omegaHover + _deltaOmegaF - _deltaOmegaTheta + _deltaOmegaPsi)*_RadToRPM;
                _omegaMotorTwo = (_omegaHover + _deltaOmegaF +_deltaOmegaPhi - _deltaOmegaPsi)*_RadToRPM;
                _omegaMotorThree = (_omegaHover + _deltaOmegaF + _deltaOmegaTheta + _deltaOmegaPsi )*_RadToRPM;
                _omegaMotorFour = (_omegaHover + _deltaOmegaF - _deltaOmegaPhi - _deltaOmegaPsi)*_RadToRPM;

                foreach (Rotor rotor in _drone.Rotors.Values)
                {
                    switch (rotor.DisplayName)
                    {
                        case "RightRotor":
                            rotor.TargetRPM = (int)Math.Ceiling(_omegaMotorOne);
                            break;
                        case "FrontRotor":
                            rotor.TargetRPM = (int)Math.Ceiling(_omegaMotorFour);
                            break;
                        case "RearRotor":
                            rotor.TargetRPM = (int)Math.Ceiling(_omegaMotorTwo);
                            break;
                        case "LeftRotor":
                            rotor.TargetRPM = (int)Math.Ceiling(_omegaMotorThree);
                            break;
                    }
                }

            }
        }

        public float TargetZAccel { get; set; }

        public float TargetYaw { get; set; }

        public float TargetPitch { get; set; }

        public float TargetRoll { get; set; }

        private void AltitudeControl(float timeStep)
        {
            _deltaOmegaF = _droneMass / (8 * _zParameter.Coefficients.Kp * _omegaHover) * TargetZAccel;
        }

        private void YawControl(float timeStep)
        {
            //Get Dist from 0
            /*float actualDist = (float)(Math.Abs(_drone.CurrentYaw) > (360f - _drone.CurrentYaw) ? (360f - _drone.CurrentYaw) : Math.Abs(_drone.CurrentYaw));
            float wantedDist = (float)(Math.Abs(TargetYaw) > (360f - TargetYaw) ? (360f - TargetYaw) : Math.Abs(TargetYaw));

            float upstream = (actualDist < wantedDist ? _drone.CurrentYaw : TargetYaw);
            float downstream = (actualDist > wantedDist ? _drone.CurrentYaw : TargetYaw);
            float yawErrorOne = upstream - downstream;
            float yawErrorTwo = upstream + (360 - downstream);
            float yawError = (yawErrorOne < yawErrorTwo ? -yawErrorTwo : yawErrorOne)/(180f/(float)Math.PI);
            */

            float yawError = TargetYaw - _drone.anglularV.Y;
            _deltaOmegaPsi = _psiParameter.Coefficients.Kp * (yawError) +
            _psiParameter.Coefficients.Kd * ((yawError - _previousYawError) / timeStep);
            _previousYawError = yawError;

        }

        private void PitchControl(float timeStep)
        {   
            
            float rollError = TargetRoll - _drone.CurrentRoll;
            _deltaOmegaTheta = _thetaParameter.Coefficients.Kp * (rollError) +
                _thetaParameter.Coefficients.Kd * ((rollError - _previousRollError) / timeStep);
            _previousRollError = rollError;
        }

        private void RollControl(float timeStep)
        {
            float pitchError = TargetPitch - _drone.CurrentPitch;
            _deltaOmegaPhi = _phiParameter.Coefficients.Kp * (pitchError) +
                _phiParameter.Coefficients.Kd * ((pitchError - _previousPitchError) / timeStep);
            _previousPitchError = pitchError;
        }

        public void StartDroneEngines()
        {
            _droneCommand.StartEngines(_gravity);
        }

        public void StopDroneEngines()
        {
            _droneCommand.StopEngines();
        }
    }
}
