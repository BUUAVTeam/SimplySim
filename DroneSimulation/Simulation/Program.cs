using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimplySim.Xna.Engine;

namespace DroneSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application (new Window(1024, 768, "Drone Simulation"));
            // Create a new simulation instance
            Simulation simulation = new Simulation(app.MainWindow);
            // Run the simulation
            app.Run(simulation);
        }
    }
}
