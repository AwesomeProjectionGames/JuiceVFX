#nullable enable

using System;
using System.Collections.Generic;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Lightweight observable property wrapper for MVVM data binding.
    /// Raises <see cref="ValueChanged"/> only when the new value differs from the current one.
    /// </summary>
    public sealed class Observable<T>
    {
        private T _value;

        public event Action<T>? ValueChanged;

        public Observable(T initialValue = default!)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    ValueChanged?.Invoke(_value);
                }
            }
        }

        /// <summary>
        /// Forces a notification even if the value hasn't changed.
        /// Useful after mutating the inner state of a reference-type value.
        /// </summary>
        public void NotifyChanged()
        {
            ValueChanged?.Invoke(_value);
        }

        public static implicit operator T(Observable<T> observable) => observable._value;

        public override string ToString() => _value?.ToString() ?? "null";
    }
}
