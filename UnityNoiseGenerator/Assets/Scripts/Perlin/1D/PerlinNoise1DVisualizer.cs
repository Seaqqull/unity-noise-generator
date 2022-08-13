using NoiseGenerator.Perlin.OneDimensional.Data;
using System.Collections.Generic;
using NoiseGenerator.Base;
using System.Collections;
using System.Linq;
using UnityEngine;


namespace NoiseGenerator.Perlin
{
    public class PerlinNoise1DVisualizer : BaseTextureContainer
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
        private static readonly int SHADER_SAMPLE_WiDTH = Shader.PropertyToID("SampleWidth");
        private static readonly int SHADER_MAIN_TEXTURE = Shader.PropertyToID("_MainTex");
        #endregion

        [SerializeField] private Color _backgroungColor = Color.black;
        [SerializeField] private Color _foregroundColor = Color.white;
        [Tooltip("Ratio of samples count to texture width")]
        [SerializeField] [Range(0.005f, 1.0f)] private float _sampleZoom = 1.0f;
        [SerializeField] [Range(0.0001f, 1.0f)] private float _sampleFrequency = 0.1f;// 0.995 interesting effect
        [SerializeField] private bool _realtimeUpdate;
        [Space] 
        [SerializeField] [Range(1, 10)] private int _octaves = 1;
        [SerializeField] [Range(0.1f, 10.0f)] private float _persistence = 0.1f;
        [Space] 
        [SerializeField] private float _seed;
        [SerializeField] private float _time;
        [SerializeField] [Range(0.000f, 1.0f)] private float _timeScale = 0.1f;
        [SerializeField] [Range(1, 60)] private int _updateRate = 15;
        
        private Queue<NoiseSample> _noiseSamples = new ();
        private ComputeBuffer _samplesBuffer;
        private PerlinNoise1D _noise;
        private Renderer _renderer;
        private int _sampleCounter;
        private int _samplesCount;
        
        
        protected override void Awake()
        {
            base.Awake();
            
            _samplesCount = (int)Mathf.Clamp(_textureSize.x * _sampleZoom, 1.0f, _textureSize.x);
            _noise = new PerlinNoise1D();
            
            _renderer = GetComponent<Renderer>();
            _renderer.enabled = true;
            
            _renderer.material.SetTexture(SHADER_MAIN_TEXTURE, _renderedSource);
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
            
            _samplesBuffer.Dispose();
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
            if (_samplesBuffer != null)
                _samplesBuffer.Release();
            _samplesBuffer = new ComputeBuffer(_noiseSamples.Count, sizeof(float));
            _samplesBuffer.SetData(_noiseSamples.ToArray());
        
            _shader.SetBuffer(_kernelHandle, SHADER_SAMPLES_BUFFER, _samplesBuffer);
            _shader.SetInt(SHADER_SAMPLES_BUFFER_COUNT, _noiseSamples.Count);
        }

        protected override void DispatchShader()
        {
            _shader.SetInt(SHADER_SAMPLE_WiDTH, _textureSize.x / _noiseSamples.Count);
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
                    Value = _noise.Evaluate(_seed + _time + (i * _sampleFrequency), _octaves, _persistence)
                });
            }

            UpdateNoiseParameters();
            DispatchShader();
        }
        
        [ContextMenu("Visualize single")]
        public void VisualizeSingle()
        {
            // --- Manage [_samplesCount] change
            if (_samplesCount != _noiseSamples.Count)
            {
                var temporarySamples = _noiseSamples.ToList();
                // Handle decrease of [_samplesCount]
                var samplesToDelete = (temporarySamples.Count - _samplesCount);
                for (int i = 0; i < samplesToDelete; i++)
                {
                    _sampleCounter--;
                    temporarySamples.RemoveAt(temporarySamples.Count - 1);
                }
            
                // Handle increase of [_samplesCount]
                while (temporarySamples.Count != _samplesCount)
                {
                    _sampleCounter++;
                    temporarySamples.Add(new NoiseSample() {
                        Value = _noise.Evaluate(_seed + ((_samplesCount + _sampleCounter) * _sampleFrequency) + _time, _octaves, _persistence)
                    });    
                }
                
                _noiseSamples.Clear();
                foreach (var noiseSample in temporarySamples)
                    _noiseSamples.Enqueue(noiseSample);
            }
            
            // Generate next noise sample
            _sampleCounter++;
            _noiseSamples.Enqueue(new NoiseSample() {
                Value = _noise.Evaluate(_seed + ((_samplesCount + _sampleCounter) * _sampleFrequency) + _time, _octaves, _persistence)
            });
            _noiseSamples.Dequeue();
            
            // Update shader parameters & Call for material update
            UpdateNoiseParameters();
            DispatchShader();
        }
    }
}
