using Microsoft.Xna.Framework;
using SimplySim.Xna.Engine;
using SimplySim.Dynamics;
using SimplySim.Simulation.Engine;

namespace DroneSimulation
{
    /// <summary>
    /// Controller class for the pursuit camera
    /// </summary>
    internal class SmoothController : INodeController
    {
        private const float K = 1f;

        private IActor _target;

        public SmoothController(IActor target)
        {
            _target = target;
            Enabled = true;
        }

        #region INodeController Members

        public string Id
        {
            get { return "SmoothController" + _target.Name; }
        }

        public bool Enabled { get; set; }


        //Update for the controller
        public void Update(SceneNode node, EngineTime time, InputState inputs)
        {
            //Get the actual position of the camera
            Vector3 position = node.WorldTranslation;

            //Get the actual rotation of the camera
            Matrix matrix = Matrix.CreateFromQuaternion(node.WorldRotation);

            //Set the corrected translmation to the camera with a smooth effect
            node.WorldTranslation = (float)(1 - K * time.ElapsedTime.TotalSeconds) * position + K * (float)time.ElapsedTime.TotalSeconds * TargetPosition;

            //Set the rotation of the camera for always looking at the NanoBot
            node.WorldRotation = LookAt(node);


        }

        //Return the rotation in order to make the camera to zAxis at the NanoBot
        private Quaternion LookAt(SceneNode node)
        {
            Vector3 zAxis = _target.WorldPose.Translation.ToXna() - node.WorldTranslation;
            zAxis.Normalize();

            Vector3 xAxis = Vector3.Cross(Vector3.Up, zAxis);
            xAxis.Normalize();

            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

            Matrix rotationMatrix = new Matrix(xAxis.X, xAxis.Y, xAxis.Z, 0, yAxis.X, yAxis.Y, yAxis.Z, 0, zAxis.X, zAxis.Y, zAxis.Z, 0, 0, 0, 0, 1);

            return Quaternion.CreateFromRotationMatrix(rotationMatrix);
        }

        //Return the NanoBot position non influenced by its Y axis rotation
        private Vector3 TargetPosition
        {
            get
            {
                Matrix worldPose = _target.WorldPose.ToXna();

                int rotation;
                if (Vector3.Transform(Vector3.UnitY, worldPose).Y > 0)
                {
                    rotation = 1;
                }
                else
                {
                    rotation = -1;
                }
                return Vector3.Transform(new Vector3(0, 1f * rotation, 3), worldPose);
            }
        }

        #endregion
    }
}