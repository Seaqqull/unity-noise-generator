using UnityEngine;


namespace NoiseGenerator.Base
{
    public class ShaderProcessor : MonoBehaviour
    {
        #region Constants
        protected static readonly string DEFAULT_KERNEL_NAME = "CSMain";
        #endregion
        
        [SerializeField] protected ComputeShader _shader;
        [SerializeField] protected string _kernelName = DEFAULT_KERNEL_NAME;
     
        protected int _kernelHandle = -1;

        public ComputeShader ShaderInstance => _shader;
        public int KernelHandle => _kernelHandle;


        protected virtual void Awake()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("It seems your target Hardware does not support Compute Shaders.");
                return;
            }
            if (!_shader)
            {
                Debug.LogError("No shader");
                return;
            }

            _kernelHandle = _shader.FindKernel(_kernelName);
        }
    }
}