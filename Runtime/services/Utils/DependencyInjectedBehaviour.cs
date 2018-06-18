using UnityEngine;

namespace BeatThat.Service
{
    /// <summary>
    /// Convenience base class for behaviours that use dependency injection.
    /// 
    /// Alternatively, you can just call <code>DependencyInjection.InjectDependencies(this)</code>
    /// from, say, the Start method of any behaviour. 
    /// </summary>
    public class DependencyInjectedBehaviour : MonoBehaviour, DependencyInjectionEventHandler
	{
        public enum InjectOnEvent { Start = 0, Awake = 1, OnEnable = 2 }

        public InjectOnEvent m_injectOnEvent = InjectOnEvent.Start;
        public InjectOnEvent injectOnEvent { get { return m_injectOnEvent; } set { m_injectOnEvent = value;  } }

        public bool didCallInject { get; private set; }

        protected void Start()
        {
            if (this.injectOnEvent == InjectOnEvent.Start && !this.didCallInject)
            {
                InjectOnce();
            }
            DidStart();
        }
        /// <summary>
        /// Override instead of Start
        /// </summary>
        virtual protected void DidStart() { }

        protected void OnEnable()
        {
            if (this.injectOnEvent == InjectOnEvent.OnEnable && !this.didCallInject)
            {
                InjectOnce();
            }
            DidEnable();
        }
        /// <summary>
        /// Override instead of OnEnable
        /// </summary>
        virtual protected void DidEnable() { }

        protected void Awake()
        {
            if (this.injectOnEvent == InjectOnEvent.Awake && !this.didCallInject)
            {
                InjectOnce();
            }
            DidAwake();
        }
        /// <summary>
        /// Override instead of Awake
        /// </summary>
        virtual protected void DidAwake() { }

        public void InjectOnce(bool force = false)
        {
            if(this.didCallInject && !force) {
                return;
            }
            this.didCallInject = true;
            DependencyInjection.InjectDependencies(this);
        }

        /// <summary>
        /// Called by DependencyInjection.InjectDependencies when injection
        /// is postponed because services have not yet init.
        /// 
        /// DependencyInjection will queue the target component and 
        /// retry injection when the services are ready.
        /// </summary>
        virtual public void OnDependencyInjectionWaitingForServicesReady()
        {
#if UNITY_EDITOR || DEBUG_UNSTRIP
            Debug.LogWarning("[" + Time.frameCount + "][" + this.name + "] OnDependencyInjectionWaitingForServicesReady");
#endif
        }

        /// <summary>
        /// Called immediatedly before dependencies are injected
        /// </summary>
        virtual public void OnWillInjectDependencies() {}

        /// <summary>
        /// Called immediatedly after dependencies are injected
        /// </summary>
        virtual public void OnDidInjectDependencies() {}
    }
}
