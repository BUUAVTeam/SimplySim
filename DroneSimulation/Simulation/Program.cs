using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimplySim.Xna.Engine;
using System.Threading;

namespace DroneSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application (new Window(1024, 768, "Drone Simulation"));
            // Create a new simulation instance

            // Create the capture window
            CaptureWindow captureWindow = new CaptureWindow(640, 480);
            // Create a engine window from this capture window
            EngineWindow captureEngineWindow = app.CreateWindow(captureWindow);

            Simulation simulation = new Simulation(app.MainWindow, captureWindow, captureEngineWindow);
            // Run the simulation

            


        
            app.Run(simulation);
        }
    }
}
