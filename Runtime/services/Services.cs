using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace BeatThat.Service
{

	/// <summary>
	/// <c>Services</c> provides access to global services by interface, e.g. Services.Require<MyServiceInterface>().
	///
	/// Services can be registered explicitly but often the most convenient way is via attribute, e.g.
	///
	/// <c>
	///
	/// public interface MyApi {}
	///
	/// /// attribute tells service system to create an instance of MyApiImpl and register it as MyApi
	/// [RegisterService(typeof(MyApi)] 
	/// public class MyApiImpl : MonoBehaviour, MyApi {}
	///
	/// ///somewhere
	/// Services.Init(() => {
	/// 	Services.Require<MyApi>(); // returns singleton MyApiImpl
	/// });
	///
	///
	/// </c>
	/// </summary>
	public class Services : MonoBehaviour
	{
		private static bool SELF_LOAD_ON_AWAKE = true;

		public static void DisableSelfLoad()
		{
			SELF_LOAD_ON_AWAKE = false;
		}

		public static Services Get
		{
			get {
				if(INSTANCE == null && (INSTANCE = UnityEngine.Object.FindObjectOfType<Services>()) == null) {
					INSTANCE = new GameObject("Services").AddComponent<Services>();
				}
				return INSTANCE;
			}
		}

        public static UnityEvent<Services> InitStatusUpdated {  get { return (m_initStatusUpdated != null)? m_initStatusUpdated: (m_initStatusUpdated = new ServicesEvent()); } }
        class ServicesEvent : UnityEvent<Services> { }
        private static ServicesEvent m_initStatusUpdated;

        public static void ShutdownAll()
        {
            if (INSTANCE != null)
            {
                INSTANCE.SetInitStatus(false);

                Destroy(INSTANCE.gameObject);
                INSTANCE = null;
            }
        }

        void OnDestroy()
        {
            this.isInitInProgress = false;
            SetInitStatus(false);
        }

        /// <summary>
        /// Called by ServiceLoader to update init-status props (isInitInProgress and hasInit)
        /// </summary>
        internal void InitStarted()
        {
            if(this.isInitInProgress)
            {
                return;
            }
            SetInitStatus(this.hasInit, true);
        }

        /// <summary>
        /// Called by ServiceLoader to update init-status props (isInitInProgress and hasInit)
        /// </summary>
        internal void InitComplete()
        {
            if(!this.isInitInProgress && this.hasInit)
            {
                return;
            }
            SetInitStatus(true);
        }

        private void SetInitStatus(bool hasInit, bool isInitInProgress = false)
        {
            if(this.hasInit == hasInit && this.isInitInProgress == isInitInProgress)
            {
                return;
            }

            this.hasInit = hasInit;
            this.isInitInProgress = isInitInProgress;

            if(m_initStatusUpdated != null)
            {
                m_initStatusUpdated.Invoke(this);
            }
        }

        public bool isInitInProgress { get; private set; }

        /// <summary>
        /// True if Service initialization has completed (at least one time)
        /// </summary>
        public bool hasInit { get; private set; }

		/// <summary>
		/// Default init op. Handy especially for integration tests
		/// </summary>
		///
		/// <param name="callback">(optional) callback invoked when all services have completed init
		/// 	(services that implement AsyncInitService may not init immediately)
		/// </param>
		public static void Init(Action callback = null)
		{
            if (!Services.exists)
            {
                ServiceLoader.FindOrCreate<ServiceLoader>().LoadServices(true, callback);
                return;
            }

            if (Services.Get.hasInit) {
                if (callback != null)
                {
                    callback();
                }
                return;
            }

            if(Services.Get.isInitInProgress) {
                if(callback == null) {
                    return;
                }

                Services.InitStatusUpdated.AddListener(s => { callback(); });
                return;
            }

            ServiceLoader.FindOrCreate<ServiceLoader>().LoadServices(true, callback);
			
		}

		/// <summary>
		/// Locate a service by registration interface and throw an exception if it cannot be located.
		/// </summary>
		public static ServiceType Require<ServiceType>() where ServiceType : class
		{
			var s = Locate<ServiceType>();
			if(s == null) {
				throw new MissingRequiredServiceException(typeof(ServiceType));
			}
			return s;
		}

		/// <summary>
		/// Locate a service by registration interface.
		/// </summary>
		/// <typeparam name="ServiceType">The 1st type parameter.</typeparam>
		public static ServiceType Locate<ServiceType>() where ServiceType : class
		{
			return Services.Get.GetService<ServiceType>();
		}

		/// <summary>
		/// Locate a service by registration interface.
		/// </summary>
		public static object Locate(Type type) 
		{
			return Services.Get.GetService(type);
		}

		/// <summary>
		/// Perform an action on a service ONLY if the service is available,
		/// i.e. if the application is quitting, ignore and avoid errors
		/// </summary>
		public static void IfAvailable<ServiceType>(Action<ServiceType> action) where ServiceType : class
		{
			var s = Locate<ServiceType>();
			if(s == null) {
				return;
			}

			action(s);
		}


		public static void SetContext(string ctx)
		{
			Services.Get.SetActiveContext(ctx);
		}

		public void SetActiveContext(string ctx)
		{
			ctx = string.IsNullOrEmpty (ctx) ? DEFAULT_CONTEXT_NAME : ctx.ToLower();

			Context ctxObj;
			if(!m_contextsByName.TryGetValue(ctx, out ctxObj)) {
				ctxObj = new Context(ctx);

				m_contextsByName[ctx] = ctxObj;

#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.Log("[" + Time.time + "] " + GetType()
				          + "::SetLocatorContext creating new context '" + ctx + "'");
#endif

            }

			this.activeContext = ctx;
			this.activeContextObj = ctxObj;
		}

		public string activeContext
		{
			get; private set;
		}

		public static bool isEnabled
		{
			get {
				return INSTANCE != null;
			}
		}

		public static bool exists
		{
			get {
				return INSTANCE != null;
			}
		}

		void Awake()
		{
			SetActiveContext(this.activeContext);
			DontDestroyOnLoad(this.gameObject);
			Services.INSTANCE = this;
			if(SELF_LOAD_ON_AWAKE) {
				var loader = new GameObject("ServiceLoader").AddComponent<ServiceLoader>();
				loader.LoadServices(true, () => Destroy (loader.gameObject));
			}
		}

		public T AddPersistentComponent<T>(string name) where T : Component
		{
			var go = new GameObject(name);
			go.transform.parent = this.transform;
			return go.AddComponent<T>();
		}

		public void RegisterService<RegistrationInterface>(ServiceRegistration serviceRegistration)
			where RegistrationInterface : class
		{
			RegisterService(serviceRegistration, typeof(RegistrationInterface));
		}

		public void RegisterService(ServiceRegistration serviceRegistration, Type registrationInterface)
		{
			// force the creation of the registered service and also make sure it's not null
			object service = serviceRegistration.GetService(this);
			if(service == null) {
				throw new NullReferenceException("ServiceRegistration returns null for type "
					+ registrationInterface.Name + ", registration=" + serviceRegistration);
			}

			var component = service as Component;
			if(component != null) {
				component.transform.parent = this.transform;
			}

			GetActiveContext().Set(registrationInterface, serviceRegistration);
		}

		public bool UnregisterService<RegistrationInterface>()
			where RegistrationInterface : class
		{
			return UnregisterService(typeof(RegistrationInterface));
		}

		public bool UnregisterService(Type registrationInterface)
		{
			object service = GetService(registrationInterface);
			if(service != null && service is Component) {
				(service as Component).transform.parent = null;
			}

			return GetActiveContext().Remove(registrationInterface);
		}

		public ServiceType GetService<ServiceType>() where ServiceType : class
		{
			Type t = typeof(ServiceType);
			ServiceRegistration registration = null;
			if(GetActiveContext().TryGet(t, out registration)) {
				return registration.GetService<ServiceType>(this);
			}
			else {
				Debug.LogWarning("No service registration for type: '" + t.ToString());
				return null;
			}
		}

		public object GetService(Type registrationInterface)
		{
			ServiceRegistration registration = null;
			if(GetActiveContext().TryGet(registrationInterface, out registration)) {
				return registration.GetService(this);
			}
			else {
				Debug.LogWarning("No service registration for type: '" + registrationInterface.ToString());
				return null;
			}
		}

		private Context GetActiveContext()
		{
			if(this.activeContextObj == null) {
				SetActiveContext(DEFAULT_CONTEXT_NAME);
			}
			return this.activeContextObj;
		}

		private Context activeContextObj
		{
			get; set;
		}

		class Context
		{
			public Context(string name)
			{
				this.name = name;
			}

			public string name
			{
				get; private set;
			}

			public bool TryGet(Type t, out ServiceRegistration r)
			{
				return m_serviceRegistrationsByType.TryGetValue(t, out r);
			}

			public void Set(Type t, ServiceRegistration r)
			{
				m_serviceRegistrationsByType[t] = r;
			}

			public bool Remove(Type t)
			{
				return m_serviceRegistrationsByType.Remove(t);
			}

			override public string ToString()
			{
				return "[Context name='" + this.name + "']";
			}

			private Dictionary<Type, ServiceRegistration> m_serviceRegistrationsByType = new Dictionary<Type, ServiceRegistration>();
		}

		private static Services INSTANCE;

		private Dictionary<string, Context> m_contextsByName = new Dictionary<string, Context>();
		private const string DEFAULT_CONTEXT_NAME = "";
	}
}
