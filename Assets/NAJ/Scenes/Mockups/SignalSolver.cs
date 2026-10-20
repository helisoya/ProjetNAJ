using UnityEngine;

namespace NAJ.Trailer
{
    public class SignalSolver : MonoBehaviour
    {
        public void SetAmbientIntensity(float value)
        {
            RenderSettings.ambientIntensity = value;
        }
    }
}
