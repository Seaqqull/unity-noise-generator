using NoiseGenerator.Utilities.Data;
using System.Collections.Generic;
using NoiseGenerator.Utilities;
using NoiseGenerator.Base;
using UnityEngine;
using System.Linq;


namespace NoiseGenerator.Functions.Noises
{
    public class NoiseMapper : TextureDrawer
    {
        [System.Serializable] public enum NoiseType { Custom, Red, Pink, White, Blue, Violet }

        
        #region Constants
        private static readonly int SHADER_SAMPLES_BUFFER_COUNT = Shader.PropertyToID("SamplesBufferCount");
        private static readonly int SHADER_COLOR_FOREGROUND = Shader.PropertyToID("ForegroundColor");
        private static readonly int SHADER_COLOR_BACKGROUND = Shader.PropertyToID("BackgroundColor");
        private static readonly int SHADER_SAMPLES_BUFFER = Shader.PropertyToID("SamplesBuffer");
        private const float MIN_DRAW_RANGE = 0.1f;
        private const float MAX_DRAW_RANGE = 0.9f;
        #endregion
        

        [SerializeField] private NoiseType _type;
        [SerializeField] private Color _backgroungColor = Color.black;
        [SerializeField] private Color _foregroundColor = Color.white;
        
        private ComputeBuffer _samplesBuffer;

        private List<NoiseSample> _noiseSamples = new ();
        private List<float> _unalignedSamples;
        private float _amplitude = 1.0f;
        private float[] _amplitudes;
        private float _amplitudeMin;
        private float _amplitudeSum;
        
        public float Amplitude
        {
            set => _amplitude = value;
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _samplesBuffer.Dispose();
        }
        
        
        private void UpdateNoiseParameters()
        {
            _shader.UpdateBuffer(_kernelHandle, SHADER_SAMPLES_BUFFER, ref _samplesBuffer, _noiseSamples, sizeof(float));
            _shader.SetInt(SHADER_SAMPLES_BUFFER_COUNT, _noiseSamples.Count);
        }

        protected override void DispatchShader()
        {
            _shader.SetVector(SHADER_COLOR_FOREGROUND, _foregroundColor);
            _shader.SetVector(SHADER_COLOR_BACKGROUND, _backgroungColor);

            base.DispatchShader();
        }

        protected virtual float MapManually(float value)
        {
            return 1.0f;
        }
        
        protected float[] ProcessSequenceSamples(List<List<float>> samples, float[] amplitudes, int size)
        {
            // Sample:Row frequencies
            // Sample:Column value
            var output = new float[size];
            for (int i = 0; i < samples.Count; i++) // Iterate over sequence
                for (int j = 0; j < size; j++) // Iterate over sequence's values
                    output[j] += (amplitudes[i % amplitudes.Length] * samples[i][j % samples[i].Count]);

            return output;
        }
        
        protected float ProcessSequenceSample(List<float> samples, float[] amplitudes)
        {
            var output = 0.0f;
                for (int i = 0; i < samples.Count; i++)
                    output += (amplitudes[i % amplitudes.Length] * samples[i]);

            return output;
        }
        
        
        public float Map(float value)
        {
            switch (_type)
            {
                case NoiseType.Custom:
                    return MapManually(value);
                case NoiseType.Red:
                    return 1.0f / value;
                case NoiseType.Pink:
                    return 1.0f / Mathf.Sqrt(value);
                case NoiseType.White:
                    return 1.0f;
                case NoiseType.Blue:
                    return Mathf.Sqrt(value);
                case NoiseType.Violet:
                    return value;
                default:
                    return 1.0f;
            }
        }

        public void MapAllValues(NoiseVisualizer origin)
        {
            _noiseSamples.Clear();

            if (_amplitudes == null)
            {
                _amplitudes = origin.Frequencies.Select(frequence => Map(frequence)).ToArray();
                _amplitudeSum = _amplitudes.Sum();
            }
            _unalignedSamples = ProcessSequenceSamples(origin.FrequenceSamples, _amplitudes, origin.Samples.Count).ToList();

            for (int i = 0; i < _unalignedSamples.Count; i++)
            {
                _noiseSamples.Add(new NoiseSample()
                {
                    Value = FloatHelper.Map(_amplitude * _unalignedSamples[i], -_amplitudeSum, _amplitudeSum, MIN_DRAW_RANGE, MAX_DRAW_RANGE)
                });
            }
            
            UpdateNoiseParameters();
            DispatchShader();
        }

        public void MapLastValue(NoiseVisualizer origin)
        {
            _unalignedSamples.RemoveAt(0);
            _unalignedSamples.Add(ProcessSequenceSample(origin.FrequenceSamples.Select(frequency => frequency[^1]).ToList(), _amplitudes));

            _noiseSamples.RemoveAt(0);
            _noiseSamples.Add(new NoiseSample()
            {
                Value = FloatHelper.Map(_amplitude * _unalignedSamples[^1], -_amplitudeSum, _amplitudeSum, MIN_DRAW_RANGE, MAX_DRAW_RANGE)
            });
            
            UpdateNoiseParameters();
            DispatchShader();
        }
    }
}
