using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DroneLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SimplySim.Xna.Engine;

namespace DroneSimulation
{
    class DroneSensorsHUD
    {
        private Drone _drone;
        private HUDManager _hudManager;
        private HUDText _positionText, _altitudeText, _yawText, _rollText, _pitchText, _engineEnabledText;

        private List<HUDText> _statusTexts;
        private Dictionary<Rotor, HUDText> _enginesTexts;

        public DroneSensorsHUD(Drone drone, HUDManager hudManager)
        {
            _drone = drone;
            _hudManager = hudManager;
            
            _enginesTexts = new Dictionary<Rotor, HUDText>();
            _statusTexts = new List<HUDText>();

            _positionText = new HUDText(new Vector2(5, 5), String.Empty, Color.Black);
            _hudManager.AddElement(_positionText);

            _altitudeText = new HUDText(new Vector2(5, 20), String.Empty, Color.Black);
            _hudManager.AddElement(_altitudeText);

            _yawText = new HUDText(new Vector2(5, 35), String.Empty, Color.Black);
            _hudManager.AddElement(_yawText);

            _pitchText = new HUDText(new Vector2(5, 50), String.Empty, Color.Black);
            _hudManager.AddElement(_pitchText);

            _rollText = new HUDText(new Vector2(5, 65), String.Empty, Color.Black);
            _hudManager.AddElement(_rollText);

            _engineEnabledText = new HUDText(new Vector2(5, 95), String.Empty, Color.Black);
            _hudManager.AddElement(_engineEnabledText);

            int i = 0;

            foreach (Rotor rotor in _drone.Rotors.Values)
            {
                ++i;
                HUDText t = new HUDText(new Vector2(5, 95 + i * 15), String.Empty, Color.Black);
                _enginesTexts.Add(rotor, t);
                _hudManager.AddElement(t);
            }
        }

        public void Update(bool isVisible)
        {
            if (isVisible)
            {
                _positionText.Text = String.Format("Position : {0:0.000} , {1:0.000}", _drone.GPS.X, _drone.GPS.Z);
                _positionText.IsVisible = true;
                _altitudeText.Text = String.Format("Altitude : {0:0.000}", _drone.GPS.Y);
                _altitudeText.IsVisible = true;
                _yawText.Text = String.Format("Yaw Angle : {0:0.000}", _drone.CurrentYaw);
                _yawText.IsVisible = true;
                _pitchText.Text = String.Format("Pitch Angle : {0:0.000}", _drone.CurrentPitch);
                _pitchText.IsVisible = true;
                _rollText.Text = String.Format("Roll Angle  : {0:0.000}", _drone.CurrentRoll);
                _rollText.IsVisible = true;

                foreach (KeyValuePair<Rotor, HUDText> rotorText in _enginesTexts)
                {
                    rotorText.Value.Text = String.Format("{0} : {1:0000}", rotorText.Key.DisplayName, rotorText.Key.CurrentRPM);
                    rotorText.Value.IsVisible = true;
                }

            }
            else if (!isVisible)
            {
                _positionText.IsVisible = false;
                _altitudeText.IsVisible = false;
                _yawText.IsVisible = false;
                _pitchText.IsVisible = false;
                _rollText.IsVisible = false;

                foreach (KeyValuePair<Rotor, HUDText> rotorText in _enginesTexts)
                {
                    rotorText.Value.IsVisible = false;
                }

            }
        }
    }
}
