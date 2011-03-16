using System;
using System.Collections.Generic;
using System.Linq;
using SimplySim.Xna.Engine;
using SimplySim.Simulation.Engine;
using SimplySim.Threading;
using SimplySim.Events;
using System.Windows;

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
using PlayerServer;

namespace DroneSimulation
{
    /// <summary>
    /// This is the main simulation class
    /// </summary>
    class Simulation : SimplySim.Simulation.Engine.InteractionsLogic
    {
        private const string BoxDroneName = "BoxDrone";

        private static readonly TimeSpan CameraRefreshRate = TimeSpan.FromMilliseconds(300);

        private bool _needCapture = true;

        private TimeSpan _cameraElapsed;

        private const string CameraIdentifier = "Camera";

        private const string BoxDroneCameraName = BoxDroneName + CameraIdentifier;
        private const string BoxDroneCameraOn = BoxDroneName + "L" + CameraIdentifier;
        private const string GlobalCameraName = "Global" + CameraIdentifier;

        

        private Drone _boxDrone;
        private KeyBoardPIDController _fourHelixBoxDroneController;
        //private PlayerController _fourHelixBoxDroneController;
        private SceneManager _scene;

        private EngineViewport _viewport;
        private EngineViewport _captureViewport;
       // private EngineViewport _downCamViewPort;


        private Dictionary<string, PerspectiveCamera> _cameras;
        private string _actualCameraName;

        private PostProcessingComponent _ssaoComponent;

        private KeyboardState _previousState;

        private DroneSensorsHUD _boxDroneHUD;

        private PlayerInteraction _player;

        private CaptureWindow _captureWindow;

        form _form1, _form2;


        /// <summary>
        /// Initializes a new instance of Simulation class with a default window and a viewport
        /// </summary>
        public Simulation(EngineWindow window, CaptureWindow captureWindow, EngineWindow captureEngineWindow)
        {
            _captureWindow = captureWindow;
            
            Path contentRoot = Path.CreateDirectory(@"..\..\..\Content").GetAbsolute();
   
          
            _viewport = window.AddViewport("Default", 1, 1, 0, 0, false);

            _captureViewport = captureEngineWindow.AddViewport("DroneCamVP", 0, 0, 1, 1, false);
           // _downCamViewPort = captureEngineWindow.AddViewport("Down Cam", 0, 0, 1, 1, false);
            
            _cameras = new Dictionary<string, PerspectiveCamera>();

            _player = new PlayerInteraction(3030);

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
            _scene.NodeAttachedFiltered.Subscribe(new RegexFilter<SceneNode>("DroneCam"), BindDroneCamera);
            _scene.NodeAttachedFiltered.Subscribe(new RegexFilter<SceneNode>("DownCam"), BindDownCamera);
            #region Drones

            // Box drone

            _boxDrone = new Drone(world, BoxDroneName, _player);
            _boxDroneHUD = new DroneSensorsHUD(_boxDrone, window.HUDManager);

            FourHelixBoxDroneCommand _droneBoxController = new FourHelixBoxDroneCommand(_boxDrone.Rotors);

            /*Parameter altParam = new Parameter(ParameterType.Altitude, new Coefficients(1, 0.1f, 0));
            Parameter yawParam = new Parameter(ParameterType.Yaw, new Coefficients(0.5f, 0.1f, 0));
            Parameter pitchParam = new Parameter(ParameterType.Pitch, new Coefficients(0.1f, 0.05f, 0.05f));
            Parameter rollParam = new Parameter(ParameterType.Roll, new Coefficients(0.1f, 0.05f, 0.05f));
            Parameter[] parameters = new Parameter[] { altParam, yawParam, pitchParam, rollParam };*/

            Parameter zParam = new Parameter(ParameterType.z, new Coefficients(0.000223f, 0.1f, 0));
            Parameter psiParam = new Parameter(ParameterType.Psi, new Coefficients(10f, 0.1f, 0.001f));
            Parameter phiParam = new Parameter(ParameterType.Phi, new Coefficients(0.5f, 0.1f, 0.001f)); //P starts at 0.2f
            Parameter thetaParam = new Parameter(ParameterType.Theta, new Coefficients(0.5f, 0.1f, 0.001f)); //P starts at 0.2f
            Parameter[] parameters = new Parameter[] { zParam, psiParam, phiParam, thetaParam };
            
           // _fourHelixBoxDroneController = new PlayerController(_player,_boxDrone, 1, worldDesc.Gravity.Length(), _droneBoxController, parameters);
            _fourHelixBoxDroneController = new KeyBoardPIDController(_boxDrone, 1, worldDesc.Gravity.Length(), _droneBoxController, parameters);
            #endregion

//            _renderPort = eng.AddViewport("VehicleCam", 1, 1, 0, 0, DroneCam);

            _captureWindow.RenderComplete += new EventHandler<RenderCompleteEventArgs>(CaptureWindowRenderComplete);

            

            world.World.ActorAddedFiltered.Subscribe(new RegexFilter<IActor> ("[.]ComplexObject[.]Body"), BindCameras);


            _scene.Enabled = true;
            world.Enabled = true;
            _form1 = new form();
            _form1.Show();

            _form2 = new form();
            _form2.Show();
        }

        /// <summary>
        /// Updates the simulation with the current EngineTime
        /// </summary>
        public override void Update(TimeSpan timeStep, InputState inputState)
        {
            KeyboardState state = Keyboard.GetState();

            //// Enables/Disables SSAO
            if (_ssaoComponent != null && _ssaoComponent.IsActive)
            {
                _ssaoComponent.IsActive = false;
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
                        _actualCameraName = GlobalCameraName;
                        break;
                }
                if (_cameras.TryGetValue(_actualCameraName, out cam))
                {
                    _viewport.Camera = cam;

                }

                
            }

           //Update for Keyboard _fourHelixBoxDroneController.Update(state, true, ((float)timeStep.Milliseconds)/1000f);
            //_fourHelixBoxDroneController.Update((float)(timeStep.Milliseconds / 1000f));
            _fourHelixBoxDroneController.Update(state, true, (float)timeStep.Milliseconds / 1000f);
            _boxDroneHUD.Update(_actualCameraName == BoxDroneCameraName);

            _previousState = state;

            _player.Update(timeStep);

            
            _cameraElapsed += timeStep;

            if (_cameraElapsed > CameraRefreshRate && _needCapture)
            {
                _cameraElapsed -= CameraRefreshRate;
                _needCapture = false;
                _captureWindow.RequestRender();
            }

            base.Update(timeStep, inputState);

        }

        private void BindDroneCamera(object sender, EventArgs<SceneNode> args)
        {
            _captureViewport.Camera = (AbstractCamera)args.Item.Entities[0];
        }

        private void BindDownCamera(object sender, EventArgs<SceneNode> args)
        {
            _captureViewport.Camera = (AbstractCamera)args.Item.Entities[0];
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
        private void CaptureWindowRenderComplete(object sender, RenderCompleteEventArgs e)
        {
           
            // Get the frame
            Color[] frame = e.Frame;

            // For example we convert the frame in a System.Drawing.Bitmap and we display it on a form
            int[] bitmapData = new int[frame.Length];

            for (int i = 0; i < frame.Length; ++i)
            {
                bitmapData[i] = frame[i].R << 16 | frame[i].G << 8 | frame[i].B;
            }

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Drawing.Imaging.BitmapData bmd32 =
                bitmap.LockBits(new System.Drawing.Rectangle(0, 0, 640, 480),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(bitmapData, 0, bmd32.Scan0, bitmapData.Length);
            }
            finally
            {
                bitmap.UnlockBits(bmd32);
            }
            //form Form = new form();
            _form1.BackgroundImage = bitmap;

            // Reset capture
            _needCapture = true;
        }
    }
}
