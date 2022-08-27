using NoiseGenerator.Base;
using UnityEngine;


namespace NoiseGenerator.Helpers
{
    public abstract class ShaderHelper : MonoBehaviour
    {
        public abstract void Process(ShaderProcessor shader);
    }
}
