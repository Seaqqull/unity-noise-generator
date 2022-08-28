using UnityEngine;


namespace NoiseGenerator.Perlin.Additional
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineHelper : MonoBehaviour
    {
        private LineRenderer _line;
        
        
        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
        }
        
        
        public void SetColor(Color color)
        {
            _line.startColor = color;
            _line.endColor = color;
        }

        public void SetColor(Color from, Color to)
        {
            _line.startColor = from;
            _line.endColor = to;
        }
        
        public void SetPoints(Vector3 positionFrom, Vector3 positionTo)
        {
            if (_line.positionCount != 2)
                _line.positionCount = 2;
            
            _line.SetPosition(0, positionFrom);
            _line.SetPosition(1, positionTo);
        }
    }
}
