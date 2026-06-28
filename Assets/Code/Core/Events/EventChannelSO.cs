using System;
using UnityEngine;

namespace ZooTycoon.Core
{
    public abstract class EventChannelSO<T> : ScriptableObject
    {
        private event Action<T> onRaised;

        public void Raise(T value) => onRaised?.Invoke(value);
        public void Subscribe(Action<T> handler) => onRaised += handler;
        public void Unsubscribe(Action<T> handler) => onRaised -= handler;
    }

    [CreateAssetMenu(menuName = "ZooTycoon/Events/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        private event Action onRaised;

        public void Raise() => onRaised?.Invoke();
        public void Subscribe(Action handler) => onRaised += handler;
        public void Unsubscribe(Action handler) => onRaised -= handler;
    }
}
