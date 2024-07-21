using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using WizardOptimizations.Runtime.Singelton;

namespace WizardOptimizations.Runtime.Timer
{
    public class Timer
    {
        /// <summary>
        /// How long the timer takes to complete from start to finish.
        /// </summary>
        public float duration { get; private set; }
        
        /// <summary>
        /// whether the timer will loop back to the start after reaching the end.
        /// </summary>
        public bool isLooped { get; set; }
        
        /// <summary>
        /// whether or not the timer completed running. this is false if the timer was cancelled
        /// </summary>
        public bool isCompleted { get; private set; }
        
        /// <summary>
        /// Whether the timer uses real-time or game-time. Real time is unaffected by changes to the timescale
        /// of the game (e.g. pausing the game). Game time is affected by changes to the timescale of the game.
        /// </summary>
        public bool usesRealTime { get; private set; }


        /// <summary>
        /// whether the timer is currently paused
        /// </summary>
        public bool isPaused
        {
            get => this.m_timeElapsedBeforePause.HasValue;
        }

        /// <summary>
        /// Whether or not the timer was cancelled.
        /// </summary>
        public bool isCancelled { get => this.m_timeElapsedBeforeCancel.HasValue; }
        
        /// <summary>
        /// Get whether or not the timer has finished running for any reason.
        /// </summary>
        public bool isDone { get => this.isCompleted || this.isCancelled || this.isOwnerDestroyed;}

        public static Timer Register(float duration, Action onComplete, Action<float> onUpdate = null,
            bool isLooped = false, bool userRealTime = false, MonoBehaviour autoDestroyOwner = null)
        {
            // create a manager object to update all the timers if one does not already exist.
            if(TimerManager.Instance == null)
            {
                return null;
            }
            
            Timer timer = new Timer(duration, onComplete, onUpdate, isLooped, userRealTime, autoDestroyOwner);
            TimerManager.Instance.RegisterTimer(timer);
            return timer;
        }
        
        public static void Cancel(Timer timer)
        {
            if(timer != null)
                timer.Cancel();
        }
        
        public static void Pause(Timer timer)
        {
            if (timer != null)
                timer.Pause();
        }
        
        public static void Resume(Timer timer)
        {
            if(timer != null)
                timer.Resume();
        }

        public static void CancelAllRegisteredTimers()
        {
            if (TimerManager.Instance != null)
                TimerManager.Instance.CancelAllTimers();
        }
        
        public static void PauseAllRegisteredTimers()
        {
            if (TimerManager.Instance != null)
                TimerManager.Instance.PauseAllTimers();
        }

        public static void ResumeAllRegisteredTimers()
        {
            if (TimerManager.Instance != null)
                TimerManager.Instance.ResumeAllTimers();
        }


        public void Cancel()
        {
            if (this.isDone)
            {
                return;
            }

            this.m_timeElapsedBeforeCancel = this.GetTimeElapsed();
            this.m_timeElapsedBeforePause = null;
        }

        public void Pause()
        {
            if (this.isPaused || this.isDone)
            {
                return;
            }

            this.m_timeElapsedBeforePause = this.GetTimeElapsed();
        }
        
        public void Resume()
        {
            if (!this.isPaused || this.isDone)
            {
                return;
            }

            this.m_timeElapsedBeforePause = null;
        }
        
        public float GetTimeElapsed()
        {
            if (this.isCompleted || this.GetWorldTime() >= this.GetFireTime())
            {
                return this.duration;
            }

            return this.m_timeElapsedBeforeCancel ??
                   this.m_timeElapsedBeforePause ??
                   this.GetWorldTime() - this.m_startTime;
        }
        
        public float GetTimeRemaining()
        {
            return this.duration - this.GetTimeElapsed();
        }
        
        public float GetRatioComplete()
        {
            return this.GetTimeElapsed() / this.duration;
        }

        public float GetRatioRemaining()
        {
            return this.GetTimeRemaining() / this.duration;
        }

        private bool isOwnerDestroyed
        {
            get { return this.m_hasAutoDestroyOwner && this.m_autoDestroyOwner == null; }
        }
        
        private readonly Action m_onComplete;
        private readonly Action<float> m_onUpdate;
        private float m_startTime;
        private float m_lastUpdateTime;
        
        private float? m_timeElapsedBeforeCancel;
        private float? m_timeElapsedBeforePause;
        
        private readonly MonoBehaviour m_autoDestroyOwner;
        private readonly bool m_hasAutoDestroyOwner;
        
        private Timer(float duration, Action onComplete, Action<float> onUpdate, bool isLooped, bool usesRealTime,
            MonoBehaviour autoDestroyOwner)
        {
            this.duration = duration;
            this.m_onComplete = onComplete;
            this.m_onUpdate = onUpdate;
            
            this.isLooped = isLooped;
            this.usesRealTime = usesRealTime;
            
            this.m_autoDestroyOwner = autoDestroyOwner;
            this.m_hasAutoDestroyOwner = autoDestroyOwner != null;

            this.m_startTime = this.GetWorldTime();
            this.m_lastUpdateTime = this.m_startTime;
        }
        
        private float GetWorldTime()
        {
            return this.usesRealTime ? Time.realtimeSinceStartup : Time.time;
        }
        
        private float GetFireTime()
        {
            return this.m_startTime + this.duration;
        }
        
        private float GetTimeDelta()
        {
            return this.GetWorldTime() - this.m_lastUpdateTime;
        }

        public void Update()
        {
            if(this.isDone)
                return;

            if (this.isPaused)
            {
                this.m_startTime += this.GetTimeDelta();
                this.m_lastUpdateTime = this.GetWorldTime();
                return;
            }
            
            this.m_lastUpdateTime = this.GetWorldTime();
            
            if(this.m_onUpdate != null)
                this.m_onUpdate(this.GetTimeElapsed());

            if (this.GetWorldTime() >= this.GetFireTime())
            {
                if(this.m_onComplete != null)
                    this.m_onComplete();

                if (this.isLooped)
                {
                    this.m_startTime = this.GetWorldTime();
                }
                else
                {
                    this.isCompleted = true;
                }
            }
        }
    }
    
}