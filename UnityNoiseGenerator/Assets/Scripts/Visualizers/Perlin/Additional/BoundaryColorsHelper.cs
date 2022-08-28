using NoiseGenerator.Helpers;
using UnityEngine;


namespace NoiseGenerator.Perlin.Additional
{
    public class BoundaryColorsHelper : ColorsShaderHelper
    {
        [SerializeField] private Color _backgroungColor = Color.black;
        
        public ColorBound[] ColorBounds
        {
            get => _noiseColorBounds;
        }
        
        
        private float Fade(float t)
        {
            return t * t * t * ( t * (t * 6 - 15) + 10);
        }


        public Color Evaluate(float value)
        {
            // <= Bottom
            if (value <= _noiseColorBounds[0].Center)
                return _noiseColorBounds[0].Color;

            // >= Top
            if (value >= _noiseColorBounds[^1].Center)
                return Color.Lerp(_noiseColorBounds[^1].Color, _backgroungColor, Fade((value - _noiseColorBounds[^1].Center) / (1.0f - _noiseColorBounds[^1].Center)));

            // Mid
            for (int i = 0; i < (_noiseColorBounds.Length - 1); i++)
            {
                if ((value >= _noiseColorBounds[i].Center) && (value <= _noiseColorBounds[i + 1].Center))
                    return Color.Lerp(_noiseColorBounds[i].Color, _noiseColorBounds[i + 1].Color, Fade((value - _noiseColorBounds[i].Center) / (_noiseColorBounds[i + 1].Center - _noiseColorBounds[i].Center)));
            }

            // Not in range
            return _backgroungColor;
        }
    }
}
