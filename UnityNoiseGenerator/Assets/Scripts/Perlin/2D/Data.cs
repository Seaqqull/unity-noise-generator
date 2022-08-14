


namespace NoiseGenerator.Perlin.TwoDimensional.Data
{
    public class PerlinNoise2D
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
        private static int ARRAY_LENGTH = 255;
        
        public int RepeatRate { get; set; }

        private int[] _instancePermutations;


        public PerlinNoise2D()
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
        

        public float Evaluate(float x, float y)
        {
            if (RepeatRate > 0)
            {
                x %= RepeatRate;
                y %= RepeatRate;
            }

            int xi = (int)x & ARRAY_LENGTH;
            int yi = (int)y & ARRAY_LENGTH;
            float xf = x - (int)x;
            float yf = y - (int)y;
            
            float v = Fade(xf);
            float u = Fade(yf);

            
            int vector_1, vector_2, vector_3, vector_4;
            
            // +-----+
            // |     |
            // +-----+
            // Top-left
            vector_1 = _instancePermutations[_instancePermutations[_instancePermutations[xi] + yi]];
            // Bottom-left
            vector_2 = _instancePermutations[_instancePermutations[_instancePermutations[xi] + IncrementCellIndex(yi)]];
            // Top-right
            vector_3 = _instancePermutations[_instancePermutations[_instancePermutations[IncrementCellIndex(xi)] + yi]];
            // Bottom-right
            vector_4 = _instancePermutations[_instancePermutations[_instancePermutations[IncrementCellIndex(xi)] + IncrementCellIndex(yi)]];

            float gradient_1, gradient_2, gradient_lerp;

            gradient_1 = Lerp(Gradient(vector_1, xf, yf), Gradient(vector_3, (xf - 1.0f), yf), v);
            gradient_2 = Lerp(Gradient(vector_2, xf, (yf - 1.0f)), Gradient(vector_4, (xf - 1.0f), (yf - 1.0f)), v);
            gradient_lerp = Lerp(gradient_1, gradient_2, u);
                
            return (gradient_lerp + 1) * 0.5f;
        }

        public float Evaluate(float x, float y, int octaves, float persistence)
        {
            float maxScaleValue = 0;
            float frequency = 1;
            float amplitude = 1;   
            float total = 0;
            
            for (int i = 0; i < octaves; i++)
            {
                total += Evaluate(x * frequency, y * frequency) * amplitude;

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

        private static float Gradient(int hash, float x, float y)
        {
            switch (hash & 0x3)
            {
                case 0x0: return x + y;
                case 0x1: return -x + y;
                case 0x2: return x - y;
                case 0x3: return -x - y;
                default: return 0;
            }
        }
        
        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
    }
}
