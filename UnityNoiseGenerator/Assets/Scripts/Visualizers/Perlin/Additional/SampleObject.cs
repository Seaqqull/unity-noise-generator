using NoiseGenerator.Base;
using UnityEngine;


namespace NoiseGenerator.Perlin.Additional
{
    public class SampleObject : BaseMono
    {
        [SerializeField] private Renderer _renderer;

        private Material _material;
        
        public Color Color
        {
            get => _material.color;
            set => _material.color = value;
        }

        
        protected override void Awake()
        {
            base.Awake();

            _material = _renderer.material;
        }
    }
}