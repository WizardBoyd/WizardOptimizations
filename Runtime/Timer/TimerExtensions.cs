using System;
using UnityEngine;

namespace WizardOptimizations.Runtime.Timer
{
    public static class TimerExtensions
    {
        public static Timer AttachTimer(this MonoBehaviour behaviour, float duration, Action onComplete,
            Action<float> onUpdate = null, bool isLooped = false, bool useRealTime = false)
        {
            return Timer.Register(duration, onComplete, onUpdate, isLooped, useRealTime, behaviour);
        }
    }
}