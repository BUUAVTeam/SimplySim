using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DroneLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using maths = SimplySim.Math;

namespace ControllersLibrary
{
    public class SixHelixSphereDroneCommand : AbstractDroneCommand
    {
        public SixHelixSphereDroneCommand(Dictionary<string, Rotor> droneRotors)
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
                    case "RearUpRotor":
                    case "FrontRightUpRotor":
                    case "FrontLeftUpRotor":
                        correctionPower = 1;
                        break;

                    case "RearDownRotor":
                    case "FrontRightDownRotor":
                    case "FrontLeftDownRotor":
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
                float correctionPower = 0;
                switch (pair.Key)
                {
                    case "RearUpRotor":
                    case "RearDownRotor":
                        correctionPower = 1;
                        break;

                    case "FrontRightUpRotor":
                    case "FrontRightDownRotor":
                    case "FrontLeftUpRotor":
                    case "FrontLeftDownRotor":
                        correctionPower = -1;
                        break;
                }
                pair.Value.TargetRPM += (int)(correctionPower * ComputeRPMCorrection(pitchTarget, pair.Value.Lift, DroneRotors.Count));
            }
        }

        public override void RollCommand(float rollTarget)
        {
            foreach (KeyValuePair<string, Rotor> pair in DroneRotors)
            {
                int correctionPower = 0;
                switch (pair.Key)
                {
                    case "FrontLeftUpRotor":
                    case "FrontLeftDownRotor":
                        correctionPower = 1;
                        break;

                    case "FrontRightUpRotor":
                    case "FrontRightDownRotor":
                        correctionPower = -1;
                        break;
                }
                pair.Value.TargetRPM += correctionPower * ComputeRPMCorrection(rollTarget, pair.Value.Lift, 4);
            }
        }
    }
}
