using System;
using UnityEngine;

public abstract class GameIntValueSource : MonoBehaviour
{
    public abstract int CurrentValue { get; }

    public event Action<int> ValueChanged;

    protected void NotifyValueChanged(int newValue)
    {
        ValueChanged?.Invoke(newValue);
    }
}