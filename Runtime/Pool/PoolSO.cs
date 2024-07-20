using System;
using System.Collections.Generic;
using UnityEngine;
using WizardOptimizations.Runtime.Factory;

namespace WizardOptimizations.Runtime.Pool
{
    /// <summary>
    /// A generic pool that generates members of type T on-demand via a factory.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements to pool.</typeparam>
    public abstract class PoolSO<T> : ScriptableObject, IPool<T>
    {
        protected readonly Stack<T> m_available = new Stack<T>();
        
        /// <summary>
        /// The factory which will be used to create <typeparamref name="T"/> on demand.
        /// </summary>
        public abstract IFactory<T> Factory { get; }
        
        protected bool bHasBeenPrewarmed { get; set; }

        protected virtual T Create()
        {
            T member = Factory.Create();
            return member;
        }

        public virtual void PreWarm(int num)
        {
            if(bHasBeenPrewarmed)
                return;
            for(int i = 0; i < num; i++)
                m_available.Push(Create());
            bHasBeenPrewarmed = true;
        }

        public virtual T Request()
        {
            T member = m_available.Count > 0 ? m_available.Pop() : Create();
            return member;
        }
        public virtual void Return(T member)
        {
            m_available.Push(member);
        }
        
        public virtual void Return(IEnumerable<T> members)
        {
            foreach(T member in members)
                Return(member);
        }
        
        public virtual IEnumerable<T> Request(int num = 1)
        {
            List<T> members = new List<T>(num);
            for(int i = 0; i < num; i++)
                members.Add(Request());
            return members;
        }

        protected virtual void OnDisable()
        {
            m_available.Clear();
            bHasBeenPrewarmed = false;
        }
    }
}