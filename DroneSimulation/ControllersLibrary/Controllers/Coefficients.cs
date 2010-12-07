namespace ControllersLibrary.Controllers
{
    public enum ParameterType
    {
        Altitude,
        Yaw,
        Pitch,
        Roll,
        Phi,
        Theta,
        Psi,
        z
    }

    public class Coefficients
    {
        public Coefficients(float kp, float kd, float ki)
        {
            Kp = kp;
            Kd = kd;
            Ki = ki;
        }

        public float Kp { get; private set; }

        public float Kd { get; private set; }

        public float Ki { get; private set; }
    }

    public class Parameter
    {
        public Parameter(ParameterType type, Coefficients coefficients)
        {
            Type = type;
            Coefficients = coefficients;
        }

        public ParameterType Type { get; private set; }

        public Coefficients Coefficients { get; private set; }
    }
}
