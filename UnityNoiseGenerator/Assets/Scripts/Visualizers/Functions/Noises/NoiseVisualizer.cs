using NoiseGenerator.Utilities.Data;
using System.Collections.Generic;
using NoiseGenerator.Utilities;
using NoiseGenerator.Base;
using System.Collections;
using UnityEngine.Events;
using System.Linq;
using UnityEngine;


namespace NoiseGenerator.Functions.Noises
{
    public class NoiseVisualizer : TextureDrawer
    {
        [System.Serializable] public class NoiseVisualizerUnityEvent : UnityEvent<NoiseVisualizer> { }
        
        
        #region Constants
        private static readonly int SHADER_SAMPLES_BUFFER_COUNT = Shader.PropertyToID("SamplesBufferCount");
        private static readonly int SHADER_COLOR_FOREGROUND = Shader.PropertyToID("ForegroundColor");
        private static readonly int SHADER_COLOR_BACKGROUND = Shader.PropertyToID("BackgroundColor");
        private static readonly int SHADER_SAMPLES_BUFFER = Shader.PropertyToID("SamplesBuffer");
        private const float MIN_DRAW_RANGE = 0.1f;
        private const float MAX_DRAW_RANGE = 0.9f;
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
        [SerializeField] [Range(1, 60)] private int _updateRate = 15;
        [Header("Frequencies")]
        [SerializeField] [Range(1, 30)] private int _frequenciesCount = 10; 
        [SerializeField] [Range(0.01f, byte.MaxValue)] private float _frequenceMaxStep = 50.0f; 
        [SerializeField] [Range(0.01f, byte.MaxValue)] private float _frequenceMinStep = 2.0f; 
        [Header("Events")]
        [SerializeField] private NoiseVisualizerUnityEvent _onVisualizeAll;
        [SerializeField] private NoiseVisualizerUnityEvent _onVisualizeSingle;
        
        private ComputeBuffer _samplesBuffer;

        private float _samplesUpdateDelay;
        private float _amplitude = 1.0f;
        private float[] _frequencies;
        private int _sampleCounter;
        private int _samplesCount;

        private List<NoiseSample> _noiseSamples = new ();
        private List<List<float>> _frequenceSamples;
        private List<float> _unalignedSamples;

        public event UnityAction<NoiseVisualizer> OnVisualizeSingle
        {
            add { _onVisualizeSingle.AddListener(value); }
            remove { _onVisualizeSingle.RemoveListener(value); }
        }
        public event UnityAction<NoiseVisualizer> OnVisualizeAll
        {
            add { _onVisualizeAll.AddListener(value); }
            remove { _onVisualizeAll.RemoveListener(value); }
        }
        public List<List<float>> FrequenceSamples
        {
            get => _frequenceSamples;
        }
        public List<NoiseSample> Samples
        {
            get => _noiseSamples;
        }
        public float Amplitude
        {
            set => _amplitude = value;
        }
        public float[] Frequencies
        {
            get => _frequencies;
        }
        

        protected override void Awake()
        {
            base.Awake();

            _frequencies = Enumerable.Range(1, _frequenciesCount).Select(value => Random.Range(_frequenceMinStep, _frequenceMaxStep)).ToArray();
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
        
        
        private void UpdateNoiseParameters()
        {
            _shader.UpdateBuffer(_kernelHandle, SHADER_SAMPLES_BUFFER, ref _samplesBuffer, _noiseSamples, sizeof(float));
            _shader.SetInt(SHADER_SAMPLES_BUFFER_COUNT, _noiseSamples.Count);
        }

        private IEnumerator VisualizeRoutine()
        {
            while (true)
            {
                float waitTime = (1.0f / _updateRate);
                yield return new WaitForSeconds(waitTime);

                if (_realtimeUpdate)
                    VisualizeSingle();
            }
        }
        
        protected override void DispatchShader()
        {
            _shader.SetVector(SHADER_COLOR_FOREGROUND, _foregroundColor);
            _shader.SetVector(SHADER_COLOR_BACKGROUND, _backgroungColor);

            base.DispatchShader();
        }
        
        protected virtual float Noise(float value)
        {
            return Mathf.Sin(value * (Mathf.PI / 180.0f));
        }
        
        protected virtual float[] Noises(float seed, float frequency, int size)
        {
            var samples = new float[size];
            for (int i = 0; i < size; i++)
            {
                samples[i] = Noise(seed + (i * frequency));
            }

            return samples;
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


        [ContextMenu("Visualize")]
        public void Visualize()
        {
            _noiseSamples.Clear();
            _frequenceSamples = new List<List<float>>();
            for (int i = 0; i < _frequencies.Length; i++)
            {
                _frequenceSamples.Add(Noises(_seed, _frequencies[i], _samplesCount).ToList());
            }

            _unalignedSamples = ProcessSequenceSamples(_frequenceSamples, new[] {1.0f}, _samplesCount).ToList();
            
            for (int i = 0; i < _samplesCount; i++)
            {
                _noiseSamples.Add(new NoiseSample()
                {
                    Value = FloatHelper.Map(_amplitude * _unalignedSamples[i], -_frequencies.Length, _frequencies.Length, MIN_DRAW_RANGE, MAX_DRAW_RANGE)
                });
            }

            UpdateNoiseParameters();
            DispatchShader();
            
            _onVisualizeAll?.Invoke(this);
        }

        [ContextMenu("Visualize single")]
        public void VisualizeSingle()
        {
            for (int i = 0; i < _frequencies.Length; i++)
            {
                // Remove the oldest amplitude's samples
                _frequenceSamples[i].RemoveAt(0);
                // Generate new sample
                _frequenceSamples[i].Add(Noises(_seed + ((_samplesCount + _sampleCounter) * _frequencies[i]), _frequencies[i], 1).ElementAt(0));
            }
            _sampleCounter++;

            
            _unalignedSamples.RemoveAt(0);
            _noiseSamples.RemoveAt(0);

            _unalignedSamples.Add(_frequenceSamples.Sum(row => row[^1]));

            _noiseSamples.Add(new NoiseSample()
            {
                Value = FloatHelper.Map(_amplitude * _unalignedSamples[^1], -_frequencies.Length, _frequencies.Length, MIN_DRAW_RANGE, MAX_DRAW_RANGE)
            });

            UpdateNoiseParameters();
            DispatchShader();
            
            _onVisualizeSingle?.Invoke(this);
        }
    }
}
