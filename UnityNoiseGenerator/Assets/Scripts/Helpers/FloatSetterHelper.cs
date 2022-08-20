using NoiseGenerator.Utilities;
using UnityEngine.Events;
using UnityEngine;
using System;


namespace NoiseGenerator.Helpers
{
    public class FloatSetterHelper : MonoBehaviour
    {
        [Serializable] private class FloatEvent : UnityEvent<float> { }


        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue = 1.0f;
        [SerializeField] private float _changeStep = 0.1f;
        [Space] 
        [SerializeField] private float _value;
        [Header("Events")] 
        [SerializeField] private FloatEvent _onValueChange;
        [SerializeField] private FloatEvent _onValueChangePercent;


        private void Awake()
        {
            _onValueChange?.Invoke(_value);
            _onValueChangePercent?.Invoke(FloatHelper.Map(_value, _minValue, _maxValue, 0.0f, 1.0f));
        }
        

        public void IncreaseValue()
        {
            _value = Mathf.Clamp(_value + _changeStep, _minValue, _maxValue);
            
            _onValueChange?.Invoke(_value);
            _onValueChangePercent?.Invoke(FloatHelper.Map(_value, _minValue, _maxValue, 0.0f, 1.0f));
        }

        public void DecreaseValue()
        {
            _value = Mathf.Clamp(_value - _changeStep, _minValue, _maxValue);
            
            _onValueChange?.Invoke(_value);
            _onValueChangePercent?.Invoke(FloatHelper.Map(_value, _minValue, _maxValue, 0.0f, 1.0f));
        }
    }
}