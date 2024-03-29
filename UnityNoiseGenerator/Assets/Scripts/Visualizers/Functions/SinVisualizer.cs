using System.Collections.Generic;
using NoiseGenerator.Utilities;
using NoiseGenerator.Base;
using System.Collections;
using UnityEngine;


namespace NoiseGenerator.Functions
{
    public class SinVisualizer : TextureDrawer
    {
        struct NoiseSample
        {
            public float Value;
        }
        
        #region Constants
        private static readonly int SHADER_SAMPLES_BUFFER_COUNT = Shader.PropertyToID("SamplesBufferCount");
        private static readonly int SHADER_COLOR_FOREGROUND = Shader.PropertyToID("ForegroundColor");
        private static readonly int SHADER_COLOR_BACKGROUND = Shader.PropertyToID("BackgroundColor");
        private static readonly int SHADER_SAMPLES_BUFFER = Shader.PropertyToID("SamplesBuffer");
        private const float MIN_DRAW_RANGE = 0.33f;
        private const float MAX_DRAW_RANGE = 0.66f;
        #endregion

        [SerializeField] private Color _backgroungColor = Color.black;
        [SerializeField] private Color _foregroundColor = Color.white;
        [Tooltip("Ratio of samples count to texture width")]
        [SerializeField] [Range(0.005f, 1.0f)] private float _sampleZoom = 1.0f;
        [SerializeField] [Range(0.0001f, 1.0f)] private float _sampleFrequency = 0.1f;// 0.995 interesting effect
        [Space] 
        [SerializeField] [Range(1, 10)] private int _octaves = 1;
        [SerializeField] [Range(0.1f, 10.0f)] private float _persistence = 0.1f;
        [Space] 
        [SerializeField] private bool _realtimeUpdate;
        [SerializeField] private float _seed;
        [SerializeField] private float _time;
        [SerializeField] [Range(0.000f, 1.0f)] private float _timeScale = 0.1f;
        [SerializeField] [Range(1, 60)] private int _updateRate = 15;
        
        private Queue<NoiseSample> _noiseSamples = new ();
        private ComputeBuffer _samplesBuffer;

        private float _samplesUpdateDelay;
        private float _amplitude = 1.0f;
        private int _samplesCount;

        
        public float Frequency
        {
            set => _sampleFrequency = value;
        }
        public float Amplitude
        {
            set => _amplitude = value;
        }


        protected override void Awake()
        {
            base.Awake();
            
            _samplesCount = (int)Mathf.Clamp(_textureSize.x * _sampleZoom, 1.0f, _textureSize.x);
        }

        private void Start()
        {
            Visualize();

            StartCoroutine(nameof(VisualizeRoutine));
        }

        private void LateUpdate()
        {
            _samplesCount = (int)Mathf.Clamp(_textureSize.x * _sampleZoom, 1.0f, _textureSize.x);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _samplesBuffer?.Dispose();
            StopCoroutine(nameof(VisualizeRoutine));
        }


        private IEnumerator VisualizeRoutine()
        {
            while (true)
            {
                float waitTime = (1.0f / _updateRate);
                yield return new WaitForSeconds(waitTime);

                if (!_realtimeUpdate)
                    continue;
                
                _time += (waitTime * _timeScale);
                
                VisualizeSingle();
            }
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


        [ContextMenu("Visualize")]
        public void Visualize()
        {
            _noiseSamples.Clear();
            for (int i = 0; i < _samplesCount; i++)
            {
                _noiseSamples.Enqueue(new NoiseSample()
                {
                    Value = FloatHelper.Map(_amplitude * Mathf.Sin(_seed + (i * _sampleFrequency) + _time), -1.0f, 1.0f, MIN_DRAW_RANGE, MAX_DRAW_RANGE)
                });
            }

            UpdateNoiseParameters();
            DispatchShader();
        }
        
        [ContextMenu("Visualize single")]
        public void VisualizeSingle()
        {
            Visualize();
        }
    }
}
