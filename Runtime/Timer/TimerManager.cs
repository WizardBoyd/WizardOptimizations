using System.Collections.Generic;
using JetBrains.Annotations;
using WizardOptimizations.Runtime.Singelton;

namespace WizardOptimizations.Runtime.Timer
{
 public class TimerManager : MonoBehaviorSingleton<TimerManager>
  {
              private List<Timer> m_timers = new List<Timer>();
      
              private List<Timer> m_timersToAdd = new List<Timer>();
              
              public void RegisterTimer(Timer timer)
              {
                  this.m_timersToAdd.Add(timer);
              }
      
              public void CancelAllTimers()
              {
                  foreach (Timer timer in this.m_timers)
                  {
                      timer.Cancel();
                  }
      
                  this.m_timers = new List<Timer>();
                  this.m_timersToAdd = new List<Timer>();
              }
      
              public void PauseAllTimers()
              {
                  foreach (Timer timer in this.m_timers)
                  {
                      timer.Pause();
                  }
              }
              
              public void ResumeAllTimers()
              {
                  foreach (Timer timer in this.m_timers)
                  {
                      timer.Resume();
                  }
              }
      
              [UsedImplicitly]
              private void Update()
              {
                  this.UpdateAllTimers();
              }
      
              private void UpdateAllTimers()
              {
                  if(this.m_timersToAdd.Count > 0)
                  {
                      this.m_timers.AddRange(this.m_timersToAdd);
                      this.m_timersToAdd.Clear();
                  }
      
                  foreach (Timer timer in this.m_timers)
                  {
                      timer.Update();
                  }
      
                  this.m_timers.RemoveAll(t => t.isDone);
              }
  }
}