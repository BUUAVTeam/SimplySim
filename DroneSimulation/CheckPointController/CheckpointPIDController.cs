using System;
using System.Collections.Generic;
using ControllersLibrary;
using ControllersLibrary.Controllers;
using DroneLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimplySim.Xna.Engine;
using maths = SimplySim.Math;

namespace CheckPointController
{
    public class Checkpoint
    {
        public Checkpoint(SceneManager scene, maths.Vector3 point, float distanceError, Color checkpointColor)
        {
            Point = point;
            DistanceError = distanceError;
            SceneNode nodePoint = scene.RootNode.CreateChildSceneNode();

            VisualPoint = new EllipsoidPrimitive(checkpointColor);

            nodePoint.AttachEntity(VisualPoint);
            nodePoint.WorldTranslation = new Vector3(point.X, point.Y, point.Z);
            nodePoint.WorldScale = Vector3.One * 2 * distanceError;
            VisualPoint.IsVisible = false;
        }
        public maths.Vector3 Point { get; private set; }
        public float DistanceError { get; private set; }
        public EllipsoidPrimitive VisualPoint { get; private set; }
    }

    public class CheckpointPIDController : SimplePIDController
    {
        private const int MaxTargetPitch = 30;
        private const int MaxTargetRoll = 30;

        private const int DistanceTargetPitch = 10;
        private const int DistanceTargetRoll = 10;

        private KeyboardState _previousState;
        private List<Checkpoint> _targetCheckpoints;
        private int _actualCheckpoint;

        private bool _isStarted, _rotorsEnabled;

        public CheckpointPIDController(Drone drone, float mass, float gravity, AbstractDroneCommand droneCommand, Parameter[] parameters, List<Checkpoint> checkpoints)
            : base(drone, mass, gravity, droneCommand, parameters)
        {
            _targetCheckpoints = checkpoints;
            _isStarted = false;
            _rotorsEnabled = false;
        }

        private void ResetController()
        {
            _actualCheckpoint = 0;
            foreach (Checkpoint cp in _targetCheckpoints)
            {
                cp.VisualPoint.IsVisible = false;
            }
        }

        public void Update(KeyboardState state, bool isControlAllowed, float timeStep)
        {
            if (isControlAllowed)
            {
                if (state.IsKeyUp(Keys.E) && _previousState.IsKeyDown(Keys.E))
                {
                    _rotorsEnabled = true;
                    ResetController();
                }
            }
            if (_isStarted && _rotorsEnabled)
            {
                base.Update(timeStep);
                _rotorsEnabled = !GoToCheckpoint();
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
            _previousState = state;
        }

        private bool GoToCheckpoint()
        {
            bool checkpointsDone = false;
            if (_actualCheckpoint <= _targetCheckpoints.Count - 1)
            {
                _targetCheckpoints[_actualCheckpoint].VisualPoint.IsVisible = true;
                maths.Vector3 errorVector = _targetCheckpoints[_actualCheckpoint].Point - base.Drone.GPS;

                //Altitude target
                base.TargetAltitude = _targetCheckpoints[_actualCheckpoint].Point.Y;

                //Yaw target
                float distance = errorVector.Length();
                maths.Vector3 direction = errorVector;
                direction.Normalize();
                if (distance > 0.5f)
                {
                    float errorYaw = (float)Math.Atan2(direction.X, -direction.Z) * 180 / (float)Math.PI;
                    base.TargetYaw = errorYaw;
                }

                //Pitch target

                maths.Vector3 cosPitchAxis = new maths.Vector3(base.Drone.Pose.M13, base.Drone.Pose.M23, base.Drone.Pose.M33);
                float errorPitch = -maths.Vector3.Dot(errorVector, cosPitchAxis);

                if (Math.Abs(errorPitch) > DistanceTargetPitch)
                {
                    errorPitch = DistanceTargetPitch * Math.Sign(errorPitch);
                }

                base.TargetPitch = errorPitch * MaxTargetPitch / (float)DistanceTargetPitch;


                //Roll target
                maths.Vector3 sinRollAxis = new maths.Vector3(base.Drone.Pose.M11, base.Drone.Pose.M21, base.Drone.Pose.M31);
                float errorRoll = -maths.Vector3.Dot(errorVector, sinRollAxis);

                if (Math.Abs(errorRoll) > DistanceTargetRoll)
                {
                    errorRoll = DistanceTargetRoll * Math.Sign(errorRoll);
                }

                base.TargetRoll = errorRoll * MaxTargetRoll / (float)DistanceTargetRoll;


                if (distance < _targetCheckpoints[_actualCheckpoint].DistanceError)
                {
                    _targetCheckpoints[_actualCheckpoint].VisualPoint.IsVisible = false;
                    if (_actualCheckpoint == _targetCheckpoints.Count - 1)
                    {
                        checkpointsDone = true;
                    }
                    _actualCheckpoint++;
                }
            }
            return checkpointsDone;
        }
    }
}
