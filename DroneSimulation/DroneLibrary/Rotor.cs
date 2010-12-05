using System;
using SimplySim.Simulation.Engine;
using SimplySim.Dynamics;
using SimplySim.Events;
using maths = SimplySim.Math;
using Microsoft.Xna.Framework;

namespace DroneLibrary
{
    /// <summary>
    /// Rotor class, handles an helix actor + its corresponding joint
    /// </summary>
    public class Rotor : IActuator
    {
        private const float ConvertRadianRPM = 60f / (MathHelper.Pi * 2f);

        private const float _KOmega = 10.0f; //K omega- speed controller response model.

        private IActor _actor;
        private IMotorizedHingeJoint _joint;
        private float _torque, _baseForceApplied;
        private string _name;
        private string _displayName;
        private maths.Vector3 _axis;
        private IDynamicActor _dynamicActor;
        private int _targetRPM;
        private float _targetRPS, _currentRPS;

        public Rotor(World world, RotorDesc rotor, string cobName)
        {
            //Rotor name
            _name = rotor.Name + Guid.NewGuid().ToString();
            _displayName = rotor.Name;
            //Defines the force direction depending of the helix type
            _baseForceApplied = 1;
            if (rotor.Blade.Type == BladeType.LeftHandedBlade)
            {
                _baseForceApplied = -1;
            }

            //Defines the engine rotor direction depending of the engine power polarity
            _torque = 1;
            if (rotor.Engine.Polarity == Polarity.Negative)
            {
                _torque = -1;
                _baseForceApplied = -_baseForceApplied;
            }

            //Defines the rotor lift BASE FORCE == K thrust
            _baseForceApplied *= (rotor.MassLift * ((WorldDesc)world.Descriptor).Gravity.Length()) / (rotor.RPMLift * rotor.RPMLift);

            //Subscribes to the world for getting back the rotor's actor and joint
            world.JointAddedFiltered.Subscribe(new RegexFilter<IJoint>(cobName + "[.]" + rotor.Engine.Name), BindJoint);
            world.ActorAddedFiltered.Subscribe(new RegexFilter<IActor>(cobName + "[.]ComplexObject[.]" + rotor.Blade.Name), BindActor);

            world.AddActuator(this);
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public int TargetRPM
        {
            get { return _targetRPM; }
            set
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Rotor " + _displayName + " is not initialized !");
                }
                _targetRPM = Math.Abs(value);
                _targetRPS = _targetRPM / ConvertRadianRPM;
                //Transform the given RPM into Radians per second
                _joint.TargetVelocity = _torque * _targetRPM / ConvertRadianRPM;
            }
        }

        public bool IsInitialized
        {
            get
            {
                //returns true if all the Rotor's actors and joints are initialized
                return _actor != null && _joint != null && _dynamicActor != null;
            }
        }

        public float MaxTorque
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Rotor " + _displayName + " is not initialized !");
                }
                //Return the actual joint torque
                return _joint.MaxTorque;
            }
            set
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Rotor " + _displayName + " is not initialized !");
                }
                //Set the actual joint torque
                _joint.MaxTorque = value;
            }
        }

        public float Lift
        {
            get { return Math.Abs(_baseForceApplied); }
        }

        public int CurrentRPM
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Rotor " + _displayName + " is not initialized !");
                }
                //Return the actual target velocity converted from Radians per second to Meters per minute
                return (int)(Math.Abs(_dynamicActor.LocalAngularVelocity.Y) * ConvertRadianRPM);
            }
        }

        private void BindJoint(Object sender, EventArgs<IJoint> args)
        {
            _joint = (IMotorizedHingeJoint)args.Item;
            _axis = ((MotorizedHingeJointDesc)_joint.Descriptor).Axis;
            TargetRPM = 0;
        }

        private void BindActor(Object sender, EventArgs<IActor> args)
        {
            _actor = args.Item;
            _dynamicActor = (IDynamicActor)_actor;
        }

        #region IActuator Members

        public string Name
        {
            get { return _name; }
        }

        public void Update(float timeStep)
        {
            if (IsInitialized)
            {
                //Apply a force on the actor depending of its actual velocity, its lift and the joint axis

                _currentRPS += (_KOmega * (_targetRPS - _currentRPS ))*timeStep;

                //_currentRPS = _targetRPS;

                float rpmVelocity = _currentRPS * ConvertRadianRPM;
                
                _dynamicActor.AddLocalForce(_baseForceApplied * rpmVelocity * rpmVelocity * _axis);
            }
        }

        #endregion
    }
}
