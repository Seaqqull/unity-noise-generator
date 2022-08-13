using NoiseGenerator.Base;
using UnityEngine;


namespace NoiseGenerator.Helpers
{
    public abstract class Helper : MonoBehaviour
    {
        public abstract void Process(ShaderProcessor shader);
    }
}
