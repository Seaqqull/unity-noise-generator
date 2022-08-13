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
}