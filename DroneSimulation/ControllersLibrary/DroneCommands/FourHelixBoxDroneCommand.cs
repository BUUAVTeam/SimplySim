using System.Collections.Generic;
using DroneLibrary;

namespace ControllersLibrary
{
    public class FourHelixBoxDroneCommand : AbstractDroneCommand
    {
        public FourHelixBoxDroneCommand(Dictionary<string, Rotor> droneRotors)
            : base(droneRotors)
        {
        }

        public override void AltitudeCommand(float altitudeTarget)
        {
            foreach (Rotor rotor in DroneRotors.Values)
            {
                rotor.TargetRPM = ComputeRPMCorrection(altitudeTarget, rotor.Lift, DroneRotors.Count);
            }
        }

        public override void YawCommand(float yawTarget)
        {
            foreach (KeyValuePair<string, Rotor> pair in DroneRotors)
            {
                int correctionPower = 0;
                switch (pair.Key)
                {
                    case "LeftRotor":
                    case "RightRotor":
                        correctionPower = 1;
                        break;

                    case "RearRotor":
                    case "FrontRotor":
                        correctionPower = -1;
                        break;
                }
                pair.Value.TargetRPM += correctionPower * ComputeRPMCorrection(yawTarget, pair.Value.Lift, DroneRotors.Count);
            }
        }

        public override void PitchCommand(float pitchTarget)
        {
            foreach (KeyValuePair<string, Rotor> pair in DroneRotors)
            {
                int correctionPower = 0;
                switch (pair.Key)
                {
                    case "RearRotor":
                        correctionPower = 1;
                        break;

                    case "FrontRotor":
                        correctionPower = -1;
                        break;
                }
                pair.Value.TargetRPM += correctionPower * ComputeRPMCorrection(pitchTarget, pair.Value.Lift, 2);
            }
        }

        public override void RollCommand(float rollTarget)
        {
            foreach (KeyValuePair<string, Rotor> pair in DroneRotors)
            {
                int correctionPower = 0;
                switch (pair.Key)
                {
                    case "LeftRotor":
                        correctionPower = 1;
                        break;

                    case "RightRotor":
                        correctionPower = -1;
                        break;
                }
                pair.Value.TargetRPM += correctionPower * ComputeRPMCorrection(rollTarget, pair.Value.Lift, 2);
            }
        }

    }
}



