using System;
using System.Collections.Generic;
using DroneLibrary;
using Microsoft.Xna.Framework.Graphics;
using SimplySim.Xna.Engine;
using Microsoft.Xna.Framework;
using ControllersLibrary.Controllers;

namespace ControllersLibrary
{
    public class SimplePIDController
    {
        private float _previousAltError, _previousYawError, _previousPitchError, _previousRollError;
        private float _iYaw, _iAlt, _iPitch, _iRoll;
        private Drone _drone;
        private float _droneMass;
        private float _gravity;

        private AbstractDroneCommand _droneCommand;

        private Parameter _altParameter, _yawParameter, _pitchParameter, _rollParameter;

        public SimplePIDController(Drone drone, float mass, float gravity, AbstractDroneCommand droneCommand, Parameter[] parameters)
        {
            _drone = drone;
            _droneMass = mass;
            _gravity = gravity;
            _droneCommand = droneCommand;
            _gravity = gravity;

            _altParameter = new Parameter(ParameterType.Altitude, new Coefficients(0, 0, 0));
            _yawParameter = new Parameter(ParameterType.Yaw, new Coefficients(0, 0, 0));
            _pitchParameter = new Parameter(ParameterType.Pitch, new Coefficients(0, 0, 0));
            _rollParameter = new Parameter(ParameterType.Roll, new Coefficients(0, 0, 0));

            foreach (Parameter param in parameters)
            {
                switch (param.Type)
                {
                    case ParameterType.Altitude:
                        _altParameter = param;
                        break;

                    case ParameterType.Pitch:
                        _pitchParameter = param;
                        break;

                    case ParameterType.Roll:
                        _rollParameter = param;
                        break;

                    case ParameterType.Yaw:
                        _yawParameter = param;
                        break;
                }
            }
        }

        protected Drone Drone { get { return _drone; } }

        public void Update(float timeStep)
        {
            if (_drone.IsInitialized)
            {
                AltitudeControl(timeStep);
                YawControl(timeStep);
                PitchControl(timeStep);
                RollControl(timeStep);
            }
        }

        public float TargetAltitude { get; set; }

        public float TargetYaw { get; set; }

        public float TargetPitch { get; set; }

        public float TargetRoll { get; set; }

        private void AltitudeControl(float timeStep)
        {
            float altitudeError = TargetAltitude - _drone.GPS.Y;
            float correctionAltitude = (_altParameter.Coefficients.Kp * altitudeError + _droneMass * _gravity + (_altParameter.Coefficients.Kd * (altitudeError - _previousAltError) / timeStep) + (_altParameter.Coefficients.Ki * _iAlt)) / Math.Abs(_drone.Pose.M22);
            _previousAltError = altitudeError;
            _iAlt += altitudeError * timeStep;
            _droneCommand.AltitudeCommand(correctionAltitude);
        }

        private void YawControl(float timeStep)
        {
            float yawError = TargetYaw % (float)360.0 - _drone.CurrentYaw;
            if (Math.Abs(yawError) > 180)
            {
                yawError = 360 - Math.Abs(yawError);
                if (_drone.CurrentYaw < 0)
                {
                    yawError = -yawError;
                }
            }
            float correctionYaw = ((_yawParameter.Coefficients.Kp * yawError + (_yawParameter.Coefficients.Kd * (yawError - _previousYawError) / timeStep) + (_yawParameter.Coefficients.Ki * _iYaw)) / 180 * (float)Math.PI);
            _previousYawError = yawError;
            _iYaw += yawError * timeStep;
            _droneCommand.YawCommand(correctionYaw);
        }

        private void PitchControl(float timeStep)
        {
            float pitchError = TargetPitch - _drone.CurrentPitch;
            if (Math.Abs(pitchError) > 180)
            {
                pitchError = 360 - Math.Abs(pitchError);
                if (_drone.CurrentPitch < 0)
                {
                    pitchError = -pitchError;
                }
            }
            float correctionPitch = ((_pitchParameter.Coefficients.Kp * pitchError + (_pitchParameter.Coefficients.Kd * (pitchError - _previousPitchError) / timeStep) + (_pitchParameter.Coefficients.Ki * _iPitch)) / 180 * (float)Math.PI);
            _previousPitchError = pitchError;
            _iPitch += pitchError * timeStep;
            _droneCommand.PitchCommand(correctionPitch);
        }

        private void RollControl(float timeStep)
        {
            float rollError = TargetRoll - _drone.CurrentRoll;
            if (Math.Abs(rollError) > 180)
            {
                rollError = 360 - Math.Abs(rollError);
                if (_drone.CurrentRoll < 0)
                {
                    rollError = -rollError;
                }
            }
            float correctionRoll = ((_rollParameter.Coefficients.Kp * rollError + (_rollParameter.Coefficients.Kd * (rollError - _previousRollError) / timeStep) + (_rollParameter.Coefficients.Ki * _iRoll)) / 180 * (float)Math.PI);
            _previousRollError = rollError;
            _iRoll += rollError * timeStep;
            _droneCommand.RollCommand(correctionRoll);
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
