using UnityEngine;

namespace WizardOptimizations.Runtime.Pool
{
    public abstract class ComponentPoolSO<T> : PoolSO<T> where T : Component
    {
        private Transform m_poolRoot;
        private Transform m_parent;

        public Transform Parent
        {
            get => m_parent;
            set
            {
                m_parent = value;
                PoolRoot.SetParent(m_parent);
            }
        }
        public Transform PoolRoot
        {
            get
            {
                if (m_poolRoot == null)
                {
                    m_poolRoot = new GameObject(name).transform;
                    m_poolRoot.SetParent(m_parent);
                }
                return m_poolRoot;
            }
        }
        
        public override T Request()
        {
            T member = base.Request();
            member.gameObject.SetActive(true);
            return member;
        }

        public override void Return(T member)
        {
            member.transform.SetParent(PoolRoot.transform);
            member.gameObject.SetActive(false);
            base.Return(member);
        }
        

        protected override T Create()
        {
            T newMember = base.Create();
            newMember.transform.SetParent(PoolRoot.transform);
            newMember.gameObject.SetActive(false);
            return newMember;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (PoolRoot != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(PoolRoot.gameObject);
#else
				Destroy(PoolRoot.gameObject);
#endif
            }
        }
    }
}