using NoiseGenerator.Perlin.TwoDimensional.Data;
using NoiseGenerator.Perlin.Additional;
using NoiseGenerator.Utilities;
using NoiseGenerator.Base;
using System.Collections;
using UnityEngine;


namespace NoiseGenerator.Perlin
{
    public class PerlinNoise3DTimeVisualizer : BaseMono
    {
        #region Constants
        private const string SAMPLES_PARENT = "Samples";
        private const string LINES_PARENT = "Lines";
        private readonly Color VISIBLE_COLOR = Color.white;
        #endregion
        
        [SerializeField] private Vector2Int _mapDimentions = new (10, 10);
        [SerializeField] private Vector3Int _mapSize = new (10, 2, 10);
        [Header("Objects")] 
        [SerializeField] private SampleObject _samplePrefab;
        [SerializeField] private LineHelper _linePrefab;
        [SerializeField] private BoundaryColorsHelper _colorHelper;
        [Space]
        [SerializeField] private bool _showVertices = true;
        [SerializeField] private bool _showLines = true;
        [SerializeField] private bool _interpolateColors;
        [Header("Samples")]
        [SerializeField] [Range(0.0001f, 1.0f)] private float _sampleFrequency = 0.1f;// 0.995 interesting effect
        [SerializeField] [Range(1, 10)] private int _octaves = 1;
        [SerializeField] [Range(0.1f, 10.0f)] private float _persistence = 0.1f;
        [Space] 
        [SerializeField] private bool _realtimeUpdate;
        [SerializeField] private float _seed;
        [SerializeField] private float _time;
        [SerializeField] [Range(-1.0f, 1.0f)] private float _timeScale = 0.5f; 
        [SerializeField] [Range(1, 60)] private int _updateRate = 15;

        private GameObject _samplesParent;
        private GameObject _linesParent;
        
        private SampleObject[,] _noiseSamples;
        private ComputeBuffer _samplesBuffer;
        private Transform _transform;
        private LineHelper[] _lines;

        private bool _verticesVisible = true;
        private float _samplesUpdateDelay;
        private bool _colorsInterpolated;
        private Vector3 _halfMapSize;
        private PerlinNoise3D _noise;


        protected override void Awake()
        {
            base.Awake();

            _transform = transform;

            _samplesParent = new GameObject(SAMPLES_PARENT);
            _linesParent = new GameObject(LINES_PARENT);
            _samplesParent.transform.parent = _transform;
            _linesParent.transform.parent = _transform;
            
            _noiseSamples = new SampleObject[_mapDimentions.x, _mapDimentions.x];
            _noise = new PerlinNoise3D();
            
            _halfMapSize = new Vector3(_mapSize.x * 0.5f, _mapSize.y * 0.5f, _mapSize.z * 0.5f);

            if (_showLines)
                InitializeLines();
            for (int i = 0; i < _mapDimentions.y; i++)
            {
                for (int j = 0; j < _mapDimentions.x; j++)
                {
                    _noiseSamples[i, j] = Instantiate(_samplePrefab, _samplesParent.transform);
                    _noiseSamples[i, j].Transform.localPosition = new Vector3(
                        FloatHelper.Map(j, 0.0f, _mapDimentions.x, -_halfMapSize.x, _halfMapSize.x), 
                        0.0f, 
                        FloatHelper.Map(i, 0.0f, _mapDimentions.y, -_halfMapSize.z, _halfMapSize.z));
                }
            }
        }
        
        private void Start()
        {
            Visualize();

            StartCoroutine(nameof(VisualizeRoutine));
        }
        
        protected void OnDestroy()
        {
            StopCoroutine(nameof(VisualizeRoutine));
        }

        
        private void DeleteLines()
        {
            for (int i = 0; i < _lines.Length; i++)
                Destroy(_lines[i].gameObject);
            
            _lines = null;
        }
        
        private void InitializeLines()
        {
            _lines = new LineHelper[_mapDimentions.x * _mapDimentions.y * 2 - (_mapDimentions.x + _mapDimentions.y)];

            for (int i = 0; i < _lines.Length; i++)
                _lines[i] = Instantiate(_linePrefab, _linesParent.transform);
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


        [ContextMenu("Visualize")]
        public void Visualize()
        {
            var lineIndexCounter = 0;
            if (_showLines && ((_lines == null) || (_lines.Length == 0)))
                InitializeLines();
            else if (!_showLines && !((_lines == null) || (_lines.Length == 0)))
                DeleteLines();

            for (int i = 0; i < _mapDimentions.y; i++)
            {
                for (int j = 0; j < _mapDimentions.x; j++)
                {
                    // Vertices
                    var noiseSample = _noise.Evaluate(_seed + (i * _sampleFrequency), _seed + (j * _sampleFrequency), _time, _octaves, _persistence);
                    var samplePosition = _noiseSamples[i, j].Transform.localPosition;
                    samplePosition.y = noiseSample.Map(0.0f, 1.0f, -_halfMapSize.y, _halfMapSize.y);
                    
                    _noiseSamples[i, j].Transform.localPosition = samplePosition;
                    if (!_showVertices && _verticesVisible) // Need to hide vertices
                    {
                        _noiseSamples[i, j].GameObject.SetActive(false);
                    }
                    else if ((_showVertices && !_verticesVisible) || (!_interpolateColors && _colorsInterpolated)) // Need to show vertices | Color interpolation needs to be disabled
                    {
                        _noiseSamples[i, j].GameObject.SetActive(_showVertices);
                        _noiseSamples[i, j].Color = (_interpolateColors)? _colorHelper.Evaluate(noiseSample) : VISIBLE_COLOR;
                    }
                    else if (_interpolateColors) // Color interpolation enabled 
                    {
                        _noiseSamples[i, j].Color = _colorHelper.Evaluate(noiseSample);
                    }

                    // lines
                    if (_showLines && (j != 0))
                    {
                        _lines[lineIndexCounter].SetPoints(_noiseSamples[i, j - 1].Transform.position, _noiseSamples[i, j].Transform.position);
                        if (_interpolateColors)
                            _lines[lineIndexCounter].SetColor(_noiseSamples[i, j - 1].Color, _noiseSamples[i, j].Color);
                        else
                            _lines[lineIndexCounter].SetColor(VISIBLE_COLOR);
                        
                        lineIndexCounter++;
                    }
                    if (_showLines && (i != 0))
                    {
                        _lines[lineIndexCounter].SetPoints(_noiseSamples[i - 1, j].Transform.position, _noiseSamples[i, j].Transform.position);
                        if (_interpolateColors)
                            _lines[lineIndexCounter].SetColor(_noiseSamples[i - 1, j].Color, _noiseSamples[i, j].Color);
                        else
                            _lines[lineIndexCounter].SetColor(VISIBLE_COLOR);
                        
                        lineIndexCounter++;
                    }
                }
            }
            _colorsInterpolated = _interpolateColors;
            _verticesVisible = _showVertices;
        }
        
        [ContextMenu("Visualize single")]
        public void VisualizeSingle()
        {
            Visualize();
        }
    }
}
