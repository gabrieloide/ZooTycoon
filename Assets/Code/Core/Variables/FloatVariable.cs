using UnityEngine;

namespace ZooTycoon.Core
{
    [CreateAssetMenu(menuName = "ZooTycoon/Variables/Float Variable")]
    public class FloatVariable : ScriptableObject
    {
        [SerializeField] private float initialValue;

        [System.NonSerialized] public float Value;

        private void OnEnable() => Value = initialValue;

        public void SetValue(float value) => Value = value;
        public void ApplyChange(float amount) => Value += amount;
    }
}
