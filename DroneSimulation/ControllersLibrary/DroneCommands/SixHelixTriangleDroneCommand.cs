using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DroneLibrary;
using Microsoft.Xna.Framework.Graphics;
using SimplySim.Xna.Engine;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ControllersLibrary
{
    public class SixHelixTriangleDroneCommand : AbstractDroneCommand
    {
        public SixHelixTriangleDroneCommand(Dictionary<string, Rotor> droneRotors)
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
                    case "RearRightRotor":
                    case "RearLeftRotor":
                    case "FrontRotor":
                        correctionPower = 1;
                        break;

                    case "FrontRightRotor":
                    case "FrontLeftRotor":
                    case "RearRotor":
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
                    case "RearRightRotor":
                    case "RearLeftRotor":
                        correctionPower = 1;
                        break;

                    case "FrontRightRotor":
                    case "FrontRotor":
                    case "FrontLeftRotor":
                        correctionPower = -1;
                        break;
                }
                pair.Value.TargetRPM += correctionPower * ComputeRPMCorrection(pitchTarget, pair.Value.Lift, DroneRotors.Count);
            }
        }

        public override void RollCommand(float rollTarget)
        {
            foreach (KeyValuePair<string, Rotor> pair in DroneRotors)
            {
                int correctionPower = 0;
                switch (pair.Key)
                {
                    case "FrontLeftRotor":
                    case "RearLeftRotor":
                        correctionPower = 1;
                        break;

                    case "RearRightRotor":
                    case "FrontRightRotor":
                        correctionPower = -1;
                        break;
                }
                pair.Value.TargetRPM += correctionPower * ComputeRPMCorrection(rollTarget, pair.Value.Lift, 4);
            }
        }
    }
}
