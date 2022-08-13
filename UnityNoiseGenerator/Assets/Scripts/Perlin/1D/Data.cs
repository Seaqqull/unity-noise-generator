


namespace NoiseGenerator.Perlin.OneDimensional.Data
{
    public class PerlinNoise1D
    {
        /// <summary>
        /// Hash lookup table as defined by Ken Perlin.  This is a randomly
        /// </summary>
        private static readonly int[] PERMUTATIONS = { 151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };
        /// <summary>
        /// Last element in the permutations array 
        /// </summary>
        private static int LAST_ARRAY_INDEX = 255;
        
        public int RepeatRate { get; set; }

        private int[] _instancePermutations;


        public PerlinNoise1D()
        {
            Initialize();
        }

        
        private void Initialize()
        {
            _instancePermutations = new int [PERMUTATIONS.Length * 2];

            for (int i = 0; i < (PERMUTATIONS.Length * 2); i++)
            {
                _instancePermutations[i] = PERMUTATIONS[i % PERMUTATIONS.Length];
            }
        }
        

        public float Evaluate(float x)
        {
            if (RepeatRate > 0) x %= RepeatRate;

            int xi = (int)x & LAST_ARRAY_INDEX;
            float xf = x - (int)x;
            
            float v = Fade(xf);

            
            int vector_1, vector_2;
            
            // Left
            vector_1 = _instancePermutations[_instancePermutations[_instancePermutations[xi]]];
            // Right
            vector_2 = _instancePermutations[_instancePermutations[_instancePermutations[IncrementCellIndex(xi)]]];

            float gradient_1;

            gradient_1 = Lerp(Gradient(vector_1, xf), Gradient(vector_2, (xf - 1.0f)), v);

            return (gradient_1 + 1) * 0.5f;
        }

        public float Evaluate(float x, int octaves, float persistence)
        {
            float maxScaleValue = 0;
            float frequency = 1;
            float amplitude = 1;   
            float total = 0;
            
            for (int i = 0; i < octaves; i++)
            {
                total += Evaluate(x * frequency) * amplitude;

                maxScaleValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxScaleValue;
        }
        
        
        private int IncrementCellIndex(int index)
        {
            index++;
            if (RepeatRate > 0) index %= RepeatRate;

            return index;
        }
        
        
        private static float Fade(float t)
        {
            return t * t * t * ( t * (t * 6 - 15) + 10);
        }

        private static float Gradient(int hash, float x)
        {
            switch (hash & 0x1)
            {
                case 0x0: return x;
                case 0x1: return -x;
                default: return 0;
            }
        }
        
        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
    }
}
