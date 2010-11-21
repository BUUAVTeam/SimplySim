using System;
using System.Collections.Generic;
using System.Linq;
using SimplySim.Xna.Engine;
using SimplySim.Simulation.Engine;
using SimplySim.Threading;
using SimplySim.Events;

using xna = Microsoft.Xna.Framework;
using maths = SimplySim.Math;
using SimplySim.Dynamics;
using Microsoft.Xna.Framework;
using SimplySim.DataModel;
using Microsoft.Xna.Framework.Input;
using DroneLibrary;
using ControllersLibrary;
using Microsoft.Xna.Framework.Graphics;
using ControllersLibrary.Controllers;
using SimplySim.IO;
using CheckPointController;

namespace DroneSimulation
{
    /// <summary>
    /// This is the main simulation class
    /// </summary>
    class Simulation : SimplySim.Simulation.Engine.InteractionsLogic
    {
        private const string SphereDroneName = "SphereDrone";
        private const string BoxDroneName = "BoxDrone";
        private const string TriangleDroneName = "TriangleDrone";

        private const string CameraIdentifier = "Camera";

        private const string SphereDroneCameraName = SphereDroneName + CameraIdentifier;
        private const string BoxDroneCameraName = BoxDroneName + CameraIdentifier;
        private const string TriangleDroneCameraName = TriangleDroneName + CameraIdentifier;
        private const string GlobalCameraName = "Global" + CameraIdentifier;

        private Drone _boxDrone, _sphereDrone, _triangleDrone;
        private KeyBoardPIDController _sixHelixTriangleDroneController, _fourHelixBoxDroneController;
        private CheckpointPIDController _sixHelixSphereDroneController;

        private SceneManager _scene;

        private EngineViewport _viewport;

        private Dictionary<string, PerspectiveCamera> _cameras;
        private string _actualCameraName;

        private PostProcessingComponent _ssaoComponent;

        private KeyboardState _previousState;

        private DroneSensorsHUD _boxDroneHUD, _droneSphereHUD, _triangleDroneHUD;

        /// <summary>
        /// Initializes a new instance of Simulation class with a default window and a viewport
        /// </summary>
        public Simulation(EngineWindow window)
        {
            Path contentRoot = Path.CreateDirectory(@"..\..\..\Content").GetAbsolute();

            _viewport = window.AddViewport("Default", 1, 1, 0, 0, false);
            _cameras = new Dictionary<string, PerspectiveCamera>();

            SimplyEnvironment env = SimplyEnvironment.Load(contentRoot + new Path("scene.env"));

            SimplySim.Xna.Engine.Construction engineConstruction = new SimplySim.Xna.Engine.Construction(this);

            SimplySim.Simulation.Engine.Construction simulationConstruction = new SimplySim.Simulation.Engine.Construction(engineConstruction);

            WorldDesc worldDesc = simulationConstruction.GenerateWorldDesc(env);

            worldDesc.FPS = 600;
            worldDesc.MaxIter = 12;
          
            WorldHandle world = AddWorld("DroneWorld", new NewtonWorld(worldDesc));

            _scene = CreateSceneManager("Drone Simulation");
            simulationConstruction.LoadEnvironment(env, world, _scene);

            _actualCameraName = GlobalCameraName;
            PerspectiveCamera globalCamera = (PerspectiveCamera)_scene[GlobalCameraName].Entities[0];
            _viewport.Camera = globalCamera;
            _cameras.Add(GlobalCameraName, globalCamera);

            engineConstruction.AddControllers(env, _scene, _viewport);

            _scene.PostProcessingManager.ComponentAddedFiltered.Subscribe(new RegexFilter<PostProcessingComponent>("Screen Space Ambient Occlusion"), BindSSAO);

            #region Drones

            // Box drone

            _boxDrone = new Drone(world, BoxDroneName);
            _boxDroneHUD = new DroneSensorsHUD(_boxDrone, window.HUDManager);

            FourHelixBoxDroneCommand _droneBoxController = new FourHelixBoxDroneCommand(_boxDrone.Rotors);

            Parameter altParam = new Parameter(ParameterType.Altitude, new Coefficients(1, 0.1f, 0));
            Parameter yawParam = new Parameter(ParameterType.Yaw, new Coefficients(0.5f, 0.1f, 0));
            Parameter pitchParam = new Parameter(ParameterType.Pitch, new Coefficients(0.3f, 0.05f, 0.05f));
            Parameter rollParam = new Parameter(ParameterType.Roll, new Coefficients(0.3f, 0.05f, 0.05f));
            Parameter[] parameters = new Parameter[] { altParam, yawParam, pitchParam, rollParam };

            _fourHelixBoxDroneController = new KeyBoardPIDController(_boxDrone, 1, worldDesc.Gravity.Length(), _droneBoxController, parameters);

            // Sphere drone

            List<Checkpoint> listCheckpoints = new List<Checkpoint>();
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(-2.5f, 1f, 22.5f), 0.25f, Color.Green));
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(30, 10, 30), 0.5f, Color.Green));
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(5, 20, 75), 0.5f, Color.Green));
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(60, 10, -5), 0.25f, Color.Green));
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(-2.5f, 5, 22.5f), 0.25f, Color.Green));
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(-2.5f, 1f, 22.5f), 0.1f, Color.Green));
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(-2.5f, 0.5f, 22.5f), 0.1f, Color.Green));
            listCheckpoints.Add(new Checkpoint(_scene, new maths.Vector3(-2.5f, 0.16f, 22.5f), 0.1f, Color.Green));

            _sphereDrone = new Drone(world, SphereDroneName);
            _droneSphereHUD = new DroneSensorsHUD(_sphereDrone, window.HUDManager);

            SixHelixSphereDroneCommand droneSphereController = new SixHelixSphereDroneCommand(_sphereDrone.Rotors);

            altParam = new Parameter(ParameterType.Altitude, new Coefficients(1, 0.1f, 0));
            yawParam = new Parameter(ParameterType.Yaw, new Coefficients(0.5f, 0.1f, 0));
            pitchParam = new Parameter(ParameterType.Pitch, new Coefficients(0.5f, 0.1f, 0.05f));
            rollParam = new Parameter(ParameterType.Roll, new Coefficients(0.5f, 0.1f, 0.05f));
            parameters = new Parameter[] { altParam, yawParam, pitchParam, rollParam };

            _sixHelixSphereDroneController = new CheckpointPIDController(_sphereDrone, 1, worldDesc.Gravity.Length(), droneSphereController, parameters, listCheckpoints);

            // Triangle drone

            _triangleDrone = new Drone(world, TriangleDroneName);
            _triangleDroneHUD = new DroneSensorsHUD(_triangleDrone, window.HUDManager);

            SixHelixTriangleDroneCommand _droneTriangleController = new SixHelixTriangleDroneCommand(_triangleDrone.Rotors);

            altParam = new Parameter(ParameterType.Altitude, new Coefficients(1, 0.1f, 0));
            yawParam = new Parameter(ParameterType.Yaw, new Coefficients(0.5f, 0.1f, 0));
            pitchParam = new Parameter(ParameterType.Pitch, new Coefficients(0.5f, 0.1f, 0.05f));
            rollParam = new Parameter(ParameterType.Roll, new Coefficients(0.5f, 0.1f, 0.05f));
            parameters = new Parameter[] { altParam, yawParam, pitchParam, rollParam };

            _sixHelixTriangleDroneController = new KeyBoardPIDController(_triangleDrone, 1, worldDesc.Gravity.Length(), _droneTriangleController, parameters);

            #endregion

            world.World.ActorAddedFiltered.Subscribe(new RegexFilter<IActor> ("[.]ComplexObject[.]Body"), BindCameras);


            _scene.Enabled = true;
            world.Enabled = true;
        }

        /// <summary>
        /// Updates the simulation with the current EngineTime
        /// </summary>
        public override void Update(TimeSpan timeStep, InputState inputState)
        {
            KeyboardState state = Keyboard.GetState();

            //// Enables/Disables SSAO
            if (_ssaoComponent != null && state.IsKeyUp(Keys.S) && _previousState.IsKeyDown(Keys.S))
            {
                _ssaoComponent.IsActive = !_ssaoComponent.IsActive;
            }

            //Switch camera
            if (state.IsKeyUp(Keys.X) && _previousState.IsKeyDown(Keys.X))
            {
                PerspectiveCamera cam = null;

                switch (_actualCameraName)
                {
                    case GlobalCameraName:
                        _actualCameraName = BoxDroneCameraName;
                        break;

                    case BoxDroneCameraName:
                        _actualCameraName = TriangleDroneCameraName;
                        break;

                    case TriangleDroneCameraName:
                        _actualCameraName = SphereDroneCameraName;
                        break;

                    case SphereDroneCameraName:
                        _actualCameraName = GlobalCameraName;
                        break;
                }

                if (_cameras.TryGetValue(_actualCameraName, out cam))
                {
                    _viewport.Camera = cam;
                }
            }
           
            _fourHelixBoxDroneController.Update(state, _actualCameraName == BoxDroneCameraName, (float)timeStep.TotalSeconds);
            _sixHelixTriangleDroneController.Update(state, _actualCameraName == TriangleDroneCameraName, (float)timeStep.TotalSeconds);
            _sixHelixSphereDroneController.Update(state, _actualCameraName == SphereDroneCameraName, (float)timeStep.TotalSeconds);
            
            _droneSphereHUD.Update(_actualCameraName == SphereDroneCameraName);
            _boxDroneHUD.Update(_actualCameraName == BoxDroneCameraName);
            _triangleDroneHUD.Update(_actualCameraName == TriangleDroneCameraName);

            _previousState = state;

            base.Update(timeStep, inputState);
        }

        private void BindCameras(object sender, EventArgs<IActor> args)
        {
            string name = args.Item.Name.Split('.')[0];
            PerspectiveCamera droneCamera = new PerspectiveCamera(name + CameraIdentifier, 0.1f, 128.0f, xna.MathHelper.PiOver4);
            SceneNode nodeCamera = _scene.RootNode.CreateChildSceneNode();
            _cameras.Add(droneCamera.Name, droneCamera);
            nodeCamera.AttachEntity(droneCamera);
            nodeCamera.AttachController(new SmoothController(args.Item));
        }

        private void BindSSAO(object sender, EventArgs<PostProcessingComponent> args)
        {
            _ssaoComponent = args.Item;
        }
    }
}
