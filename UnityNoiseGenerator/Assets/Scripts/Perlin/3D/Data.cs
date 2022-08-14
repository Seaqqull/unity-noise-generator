


namespace NoiseGenerator.Perlin.TwoDimensional.Data
{
    public class PerlinNoise3D
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


        public PerlinNoise3D()
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
        

        public float Evaluate(float x, float y, float z)
        {
            if (RepeatRate > 0)
            {
                x %= RepeatRate;
                y %= RepeatRate;
                z %= RepeatRate;
            }

            int xi = (int)x & ARRAY_LENGTH;
            int yi = (int)y & ARRAY_LENGTH;
            int zi = (int)z & ARRAY_LENGTH;
            float xf = x - (int)x;
            float yf = y - (int)y;
            float zf = z - (int)z;
            
            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);

            
            int vector_tlf, vector_blf, vector_trf, vector_brf, vector_tlb, vector_blb, vector_trb, vector_brb;
            //    +-----+
            //   /|    /|
            //  / |   / |
            // +-----+  |
            // |  +--|--+
            // |/    | /
            // +-----+
            // Top-left-front
            vector_tlf = _instancePermutations[_instancePermutations[_instancePermutations[xi] + yi] + zi];
            // Bottom-left-front
            vector_blf = _instancePermutations[_instancePermutations[_instancePermutations[xi] + IncrementCellIndex(yi)] + zi];
            // Top-right-front
            vector_trf = _instancePermutations[_instancePermutations[_instancePermutations[IncrementCellIndex(xi)] + yi] + zi];
            // Bottom-right-front
            vector_brf = _instancePermutations[_instancePermutations[_instancePermutations[IncrementCellIndex(xi)] + IncrementCellIndex(yi)] + zi];
            // -------------
            // Top-left-back
            vector_tlb = _instancePermutations[_instancePermutations[_instancePermutations[xi] + yi] + IncrementCellIndex(zi)];
            // Bottom-left-back
            vector_blb = _instancePermutations[_instancePermutations[_instancePermutations[xi] + IncrementCellIndex(yi)] + IncrementCellIndex(zi)];
            // Top-right-back
            vector_trb = _instancePermutations[_instancePermutations[_instancePermutations[IncrementCellIndex(xi)] + yi] + IncrementCellIndex(zi)];
            // Bottom-right-back
            vector_brb = _instancePermutations[_instancePermutations[_instancePermutations[IncrementCellIndex(xi)] + IncrementCellIndex(yi)] + IncrementCellIndex(zi)];
            

            float gradient_1, gradient_2, gradient_lerp_1, gradient_lerp_2;

            // Top-left-front <=> Top-right-front
            gradient_1 = Lerp(Gradient(vector_tlf, xf, yf, zf), Gradient(vector_trf, (xf - 1.0f), yf, zf), u);
            // Bottom-left-front <=> Bottom-right-front
            gradient_2 = Lerp(Gradient(vector_blf, xf, (yf - 1.0f), zf), Gradient(vector_brf, (xf - 1.0f), (yf - 1.0f), zf), u);
            // Front-top-bottom
            gradient_lerp_1 = Lerp(gradient_1, gradient_2, v);
            
            // Top-left-back <=> Top-right-back
            gradient_1 = Lerp(Gradient(vector_tlb, xf, yf, (zf - 1.0f)), Gradient(vector_trb, (xf - 1.0f), yf, (zf - 1.0f)), u);
            // Bottom-left-back <=> Bottom-right-back
            gradient_2 = Lerp(Gradient(vector_blb, xf, (yf - 1.0f), (zf - 1.0f)), Gradient(vector_brb, (xf - 1.0f), (yf - 1.0f), (zf - 1.0f)), u);
            // Back-top-bottom
            gradient_lerp_2 = Lerp(gradient_1, gradient_2, v);


            return (Lerp(gradient_lerp_1, gradient_lerp_2, w) + 1) * 0.5f;
        }

        public float Evaluate(float x, float y, float z, int octaves, float persistence)
        {
            float maxScaleValue = 0;
            float frequency = 1;
            float amplitude = 1;
            float total = 0;
            
            for (int i = 0; i < octaves; i++)
            {
                total += Evaluate(x * frequency, y * frequency, z * frequency) * amplitude;

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

        private static float Gradient(int hash, float x, float y, float z)
        {
            switch (hash & 0xF)
            {
                case 0x0: return x + y;
                case 0x1: return -x + y;
                case 0x2: return x - y;
                case 0x3: return -x - y;
                case 0x4: return x + z;
                case 0x5: return -x + z;
                case 0x6: return x - z;
                case 0x7: return -x - z;
                case 0x8: return y + z;
                case 0x9: return -y + z;
                case 0xA: return y - z;
                case 0xB: return -y - z;
                case 0xC: return y + x;
                case 0xD: return -y + z;
                case 0xE: return y - x;
                case 0xF: return -y - z;
                default: return 0;
            }
        }
        
        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
    }
}
