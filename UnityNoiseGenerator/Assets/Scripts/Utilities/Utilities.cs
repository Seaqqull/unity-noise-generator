using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace NoiseGenerator.Utilities
{
    public static class ShaderExtensions
    {
        public static void UpdateBuffer<T>(this ComputeShader shader, int kernelHandle, int bufferID, ref ComputeBuffer buffer, IEnumerable<T> collection, int stride) where T : struct
        {
            if (buffer != null)
                buffer.Release();
            buffer = new ComputeBuffer(collection.Count(), stride);
            buffer.SetData(collection.ToArray());
            
            shader.SetBuffer(kernelHandle, bufferID, buffer);
        }
        
        public static void UpdateBuffer<T>(this ComputeShader shader, int kernelHandle, int bufferID, int bufferCountID, ref ComputeBuffer buffer, IEnumerable<T> collection, int stride) where T : struct
        {
            if (buffer != null)
                buffer.Release();
            buffer = new ComputeBuffer(collection.Count(), stride);
            buffer.SetData(collection.ToArray());
            
            shader.SetBuffer(kernelHandle, bufferID, buffer);
            shader.SetInt(bufferCountID, collection.Count());
        }
    }
    
    public static class FloatHelper
    {
        /// <summary>
        /// Maps float value from interval (istart, istop) to (ostart, ostop).
        /// </summary>
        /// <param name="value">Value to be mapped.</param>
        /// <param name="istart">Original min value.</param>
        /// <param name="istop">Original max value.</param>
        /// <param name="ostart">Relative min value.</param>
        /// <param name="ostop">Relative max value.</param>
        /// <returns>Mapped float value.</returns>
        public static float Map(this float value, float istart, float istop, float ostart, float ostop)
        {
            return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
        }
    }
}