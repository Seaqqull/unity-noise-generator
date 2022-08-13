using NoiseGenerator.Utilities;
using NoiseGenerator.Base;
using System.Linq;
using UnityEngine;
using System;


namespace NoiseGenerator.Helpers
{
    public class ColorsHelper : Helper
    {
        [Serializable]
        struct ColorBound
        {
            [Range(0.0f, 1.0f)] public float Center;
            public Color Color;
        }
        
        
        #region Constants
        private static readonly int SHADER_COLORS_BUFFER_COUNT = Shader.PropertyToID("ColorsBufferCount");
        private static readonly int SHADER_COLORS_BUFFER = Shader.PropertyToID("ColorsBuffer");
        private static readonly int BUFFER_STRIDE = sizeof(float) * 5;
        #endregion

        [SerializeField] private bool _enabled = true;
        [Space]
        [SerializeField] private ColorBound[] _noiseColorBounds;

        private ComputeBuffer _colorsBuffer;


        public override void Process(ShaderProcessor shader)
        {
            if (!_enabled) return;
                
            shader.ShaderInstance.UpdateBuffer(
                shader.KernelHandle, 
                SHADER_COLORS_BUFFER, SHADER_COLORS_BUFFER_COUNT, 
                ref _colorsBuffer,
                _noiseColorBounds.OrderBy(noiseColor => noiseColor.Center), 
                BUFFER_STRIDE
            );
        }
    }
}
