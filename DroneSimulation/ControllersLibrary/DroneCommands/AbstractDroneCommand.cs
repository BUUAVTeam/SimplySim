using System;
using System.Collections.Generic;
using DroneLibrary;

namespace ControllersLibrary
{
    public abstract class AbstractDroneCommand
    {
        private Dictionary<string, Rotor> _droneRotors;
        public bool motorStart;

        public AbstractDroneCommand(Dictionary<string, Rotor> droneRotors)
        {
            _droneRotors = droneRotors;
            motorStart = true;
        }

        protected Dictionary<string, Rotor> DroneRotors { get { return _droneRotors; } }

        public abstract void AltitudeCommand(float altitudeTarget);

        public abstract void YawCommand(float yawTarget);

        public abstract void PitchCommand(float pitchTarget);

        public abstract void RollCommand(float rollTarget);

        protected int ComputeRPMCorrection(float correctionValue, float rotorLift, int rotorCount)
        {
            return (int)Math.Ceiling(Math.Sign(correctionValue) * (Math.Sqrt(Math.Abs(correctionValue / rotorLift) / (float)rotorCount)));
        }

        public void StopEngines()
        {
            foreach (Rotor r in _droneRotors.Values)
            {
                r.TargetRPM = 0;
            }
            motorStart = false;
        }

        public void StartEngines(float gravity)
        {
            foreach (Rotor rotor in _droneRotors.Values)
            {
                int targetRPM = (int)Math.Ceiling((0.2f)*(Math.Sign(gravity) * (Math.Sqrt(Math.Abs(gravity / rotor.Lift) / (float)_droneRotors.Count))));
                rotor.TargetRPM = targetRPM;
            }
            motorStart = true;
        }
    }
}
