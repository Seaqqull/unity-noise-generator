using UnityEngine;


namespace NoiseGenerator.Base
{
    [RequireComponent(typeof(Renderer))]
    public class TextureDrawer : BaseTextureContainer
    {
        #region Constants
        private static readonly int SHADER_MAIN_TEXTURE = Shader.PropertyToID("_MainTex");
        #endregion

        protected Renderer _renderer;
        
        
        protected override void Awake()
        {
            base.Awake();
            
            _renderer = GetComponent<Renderer>();
            _renderer.enabled = true;
            
            _renderer.material.SetTexture(SHADER_MAIN_TEXTURE, _renderedSource);
        }
    }
}