using UnityEngine;


namespace NoiseGenerator.Base
{
    public class BaseTextureContainer : ShaderProcessor
    {
        #region Constants
        protected static readonly int SHADER_TEXTURE_RESOLUTION = Shader.PropertyToID("TextureResolution");
        protected static readonly int SHADER_RESULT = Shader.PropertyToID("Result");
        #endregion
        
        [Space]
        [SerializeField] protected Vector2Int _textureSize = new (256, 256);
     
        protected RenderTexture _renderedSource;
        protected Vector2Int _groupSize;
        

        protected override void Awake()
        {
            base.Awake();
            
            _shader.GetKernelThreadGroupSizes(_kernelHandle, out  var x, out var y, out _);
            _groupSize.x = Mathf.CeilToInt((float)_textureSize.x / x);
            _groupSize.y = Mathf.CeilToInt((float)_textureSize.y / y);

            CreateTextures();
        }
        
        protected virtual void OnDestroy()
        {
            ClearTextures();
        }
        

        protected virtual void ClearTextures()
        {
            ClearTexture(ref _renderedSource);
        }

        protected virtual void CreateTextures()
        {
            CreateTexture(out _renderedSource);
            
            _shader.SetTexture(_kernelHandle, SHADER_RESULT, _renderedSource);
            _shader.SetInts(SHADER_TEXTURE_RESOLUTION, new [] { _textureSize.x, _textureSize.y });
        }
        
        protected virtual void DispatchShader()
        {
            _shader.Dispatch(_kernelHandle, _groupSize.x, _groupSize.y, 1);
        }
        
        protected void ClearTexture(ref RenderTexture textureToClear)
        {
            if (null == textureToClear)
                return;

            textureToClear.Release();
            textureToClear = null;
        }
        
        protected void CreateTexture(out RenderTexture textureToMake, int divide = 1)
        {
            textureToMake = new RenderTexture(_textureSize.x / divide, _textureSize.y / divide, 0);
            textureToMake.enableRandomWrite = true;
            
            textureToMake.Create();
        }
    }
}
