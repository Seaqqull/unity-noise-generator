using NoiseGenerator.Perlin.TwoDimensional.Data;
using System.Collections.Generic;
using NoiseGenerator.Utilities;
using NoiseGenerator.Base;
using UnityEngine.Events;
using System.Collections;
using UnityEngine;


namespace NoiseGenerator.Perlin
{
    public class PerlinNoise2DTimeVisualizer : BaseTextureContainer
    {
        [System.Serializable] public class ShaderProcessorUnityEvent : UnityEvent<ShaderProcessor> { }
        struct NoiseSample
        {
            public float Value;
        }


        #region Constants
        private static readonly int SHADER_SAMPLES_BUFFER_COUNT = Shader.PropertyToID("SamplesBufferCount");
        private static readonly int SHADER_COLOR_FOREGROUND = Shader.PropertyToID("ForegroundColor");
        private static readonly int SHADER_COLOR_BACKGROUND = Shader.PropertyToID("BackgroundColor");
        private static readonly int SHADER_SAMPLES_BUFFER = Shader.PropertyToID("SamplesBuffer");
        private static readonly int SHADER_FIELD_COLUMNS = Shader.PropertyToID("FieldRows");
        private static readonly int SHADER_FIELD_ROWS = Shader.PropertyToID("FieldColumns");
        private static readonly int SHADER_MAIN_TEXTURE = Shader.PropertyToID("_MainTex");
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
        [SerializeField] [Range(-1.0f, 1.0f)] private float _timeScale = 0.5f; 
        [SerializeField] [Range(1, 60)] private int _updateRate = 15;
        [Header("Events")]
        [SerializeField] private PerlinNoise1DTimeVisualizer.ShaderProcessorUnityEvent _onStart;
        [SerializeField] private PerlinNoise1DTimeVisualizer.ShaderProcessorUnityEvent _onDispatch;
        
        private Queue<NoiseSample> _noiseSamples = new ();
        private ComputeBuffer _samplesBuffer;

        private float _samplesUpdateDelay;
        private PerlinNoise3D _noise;
        private Renderer _renderer;
        private int _columns;
        private int _rows;
        
        
        public event UnityAction<ShaderProcessor> OnDispatch
        {
            add { _onDispatch.AddListener(value); }
            remove { _onDispatch.RemoveListener(value); }
        }
        public event UnityAction<ShaderProcessor> OnStart
        {
            add { _onStart.AddListener(value); }
            remove { _onStart.RemoveListener(value); }
        }
        
        
        protected override void Awake()
        {
            base.Awake();
            
            _columns = (int)Mathf.Clamp(_textureSize.x * _sampleZoom, 1.0f, _textureSize.x);
            _rows = (int)Mathf.Clamp(_textureSize.y * _sampleZoom, 1.0f, _textureSize.y);

            _noise = new PerlinNoise3D();
            
            _renderer = GetComponent<Renderer>();
            _renderer.enabled = true;
            
            _renderer.material.SetTexture(SHADER_MAIN_TEXTURE, _renderedSource);
        }

        private void Start()
        {
            _onStart?.Invoke(this);
            Visualize();

            StartCoroutine(nameof(VisualizeRoutine));
        }

        private void LateUpdate()
        {
            _columns = (int)Mathf.Clamp(_textureSize.x * _sampleZoom, 1.0f, _textureSize.x);
            _rows = (int)Mathf.Clamp(_textureSize.y * _sampleZoom, 1.0f, _textureSize.y);
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
                
                _onDispatch?.Invoke(this);
                VisualizeSingle();
            }
        }

        private void UpdateNoiseParameters()
        {
            _shader.UpdateBuffer(_kernelHandle, SHADER_SAMPLES_BUFFER, ref _samplesBuffer, _noiseSamples, sizeof(float));
            _shader.SetInt(SHADER_SAMPLES_BUFFER_COUNT, _noiseSamples.Count);
            _shader.SetInt(SHADER_FIELD_COLUMNS, _columns + 1);
            _shader.SetInt(SHADER_FIELD_ROWS, _rows + 1);
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
            
            // + 1 row, column to perform some sort of continuous effect 
            for (int i = 0; i <= _rows; i++)
            {
                for (int j = 0; j <= _columns; j++)
                {
                    _noiseSamples.Enqueue(new NoiseSample()
                    {
                        Value = _noise.Evaluate(_seed + (i * _sampleFrequency), _seed + (j * _sampleFrequency), _time, _octaves, _persistence)
                    });
                }
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
