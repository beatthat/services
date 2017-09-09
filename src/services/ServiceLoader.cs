using UnityEngine;
using System;
using System.Collections.Generic;
namespace BeatThat.Service
{
	/// <summary>
	/// Loads a games services to the Services.
	/// 
	/// Generally, each game should subclass ServiceLoader and override DoSetFactoryServiceRegistrations
	/// to install most services.
	/// 
	/// For services that need to be configure in the Unity scene, 
	/// any gameobject that is a direct child of the ServiceLoader *and* 
	/// has exactly one (non-transform) component, will be auto registered
	/// using the concrete type of that component as the registration interface.
	/// 
	/// Alternatively if you have a unity-configured service component
	/// but you want to use a registration interface that is NOT
	/// the component's concrete type, create a custom GenericMBServiceRegistration subclass for that component.
	/// </summary>
	public class ServiceLoader : MonoBehaviour, IDisposable
	{
		public bool m_disposeAfterLoad = true;

		public static bool DEBUG_LOAD_SERVICES = false;
		
		public Services m_services;
		public bool m_autoLoadOnAwake = false;
		public bool m_dieIfServicesExists = false; // if serviceloadeder is forced to live in a scene that gets loaded multiple times

		public static string RESOURCE_PATH = "Service/Services";


		public bool disposeAfterLoad { get { return m_disposeAfterLoad; } set { m_disposeAfterLoad = value; } }

		#region IDisposable implementation

		public void Dispose ()
		{
			if(GetComponent<Services>() != null) {
				// ServiceLoader is attached to services, so don't destroy gameobject
				Destroy(this);
			}
			else {
				Destroy(this.gameObject);
			}
		}

		#endregion

		public static ServiceLoader FindOrCreate(bool disableSelfLoad = true, bool disposeAfterLoad = true)
		{
			return FindOrCreate<ServiceLoader>(disableSelfLoad, disposeAfterLoad);
		}

		public static ServiceLoader FindOrCreate<T>(bool disableSelfLoad = true, bool disposeAfterLoad = true) where T : ServiceLoader
		{
			if(disableSelfLoad) {
				Services.DisableSelfLoad();
			}

			var sl = UnityEngine.Object.FindObjectOfType<T>();
			if(sl == null) {
				var asset = Resources.Load<T>(RESOURCE_PATH);
				if(asset != null) {
					sl = Instantiate(asset);
					sl.gameObject.name = "Services";

					#if UNITY_EDITOR
					sl.gameObject.AddComponent<Comment>().text = "Loaded from Resource path '" + RESOURCE_PATH + "'";
					#endif
				}
				else {
					sl = new GameObject ("ServiceLoader").AddComponent<T> ();
				}
			}

			sl.disposeAfterLoad = disposeAfterLoad;

			return sl;
		}

		void Awake() 
		{
			if(m_dieIfServicesExists) {
				if(FindObjectOfType(typeof(Services)) as Services != null) {
					Destroy(this.gameObject);
					return;
				}
			}

			if(m_autoLoadOnAwake) {
				LoadServices(false, null);
			}
		}

		protected Services services 
		{
			get {
				return EnsureServicesExists();
			}
			set {
				m_services = value;
			}
		}

		protected Services EnsureServicesExists()
		{
			if(m_services == null) {
				m_services = FindObjectOfType(typeof(Services)) as Services;
				if(m_services == null) {
					Services.DisableSelfLoad();
					m_services = new GameObject("Services").AddComponent<Services>();
				}
			}
			return m_services;
		}
		
		public static void DoDefaultInitService(ServiceRegistration reg, Services loc, Action onCompleteCallback)
		{
			object s = reg.GetService(loc);
			var asyncInitService = s as AsyncInitService;
			if(asyncInitService != null) {
				asyncInitService.InitServiceAsync(loc, onCompleteCallback);
			}
			else {
				var autoInitService = s as AutoInitService;
				if(autoInitService != null) {
					autoInitService.InitService(loc);
				}
				
				onCompleteCallback();
			}
		}
		
		public void SetServiceRegistration<RegistrationInterface>(ServiceRegistration serviceRegistration)
		{
			SetServiceRegistration(serviceRegistration, typeof(RegistrationInterface));
		}
		
		public void SetServiceRegistration(ServiceRegistration serviceRegistration, Type registrationInterface)
		{
			m_serviceRegistrationsByType[registrationInterface] = serviceRegistration;
		}
		
		public void LoadServices(bool forceReload, Action onCompleteCallback = null)
		{
			if(!m_servicesLoaded || forceReload) {
				EnsureServicesExists();
				DoLoadServices(() => {

					m_servicesLoaded = true;

					if(onCompleteCallback != null) {
						onCompleteCallback();
					}

					if(this.disposeAfterLoad) {
						Dispose();
					}
				});
			}
		}

		virtual protected void DoLoadServices(Action onCompleteCallback)
		{
			BeforeLoadServices();
			
			// separate configurations from registrations 
			// to allow app-specific overrides to occur before any services are registered
			SetServiceRegistrations();
			RegisterServices();
			InitializeServices(() => {
				
				DoStartServices();
				
				if(onCompleteCallback != null) {
					onCompleteCallback();
				}
			});
		}
		
		public void UnloadServices()
		{
			if(m_servicesLoaded)
			{
				UnregisterServices();
				m_servicesLoaded = false;
			}
		}
		
		virtual protected void BeforeLoadServices()
		{
			
		}
		
		virtual protected void SetServiceRegistrations()
		{
			FindAutowireServiceRegistrations(); // defaults

			FindFactoryServiceRegistrations(); // code based registrations in an app-specific serviceloader

			FindSceneServiceRegistrations(); // serviceloader was preconfigured into a unity scene with children that are service components

			if(this.onAfterSetServiceRegistratons != null) {
				this.onAfterSetServiceRegistratons();
			}
		}

		/// <summary>
		/// Use this to override default service registrations with a delegate. 
		/// Handy for test scenarios where you don't want to have to create a whole ServiceLoader subclass.
		/// </summary>
		/// <value>The on after service registratons.</value>
		public Action onAfterSetServiceRegistratons { get; set; }

		virtual protected void FindAutowireServiceRegistrations()
		{
			using(var wirings = ListPool<InterfaceAndImplementation>.Get()) {
				AutowiredServiceRegistrations.GetWirings(wirings);

				using(var proxies = ListPool<Type>.Get()) {
					foreach(var intfAndImpl in wirings) {
						Register(intfAndImpl.interfaceType, intfAndImpl.implType);
						if(!AutowiredServiceRegistrations.GetProxyInterfaces(intfAndImpl.interfaceType, proxies)) {
							continue;
						}

						foreach(var p in proxies) {
							new ProxyServiceRegistration(p, intfAndImpl.interfaceType).SetServiceRegistration(this);
						}
						proxies.Clear();
					}
				}
			}
		}

		virtual protected void FindFactoryServiceRegistrations()
		{
			
		}
		
		virtual protected void FindSceneServiceRegistrations()
		{
			foreach(Transform childT in this.transform) {
				ServiceRegistration reg = childT.GetComponent<ServiceRegistration>(); // TODO: should support multiple (if not why?)
				if(reg != null) {
					// this service has a custom ServiceRegistration component...
					reg.SetServiceRegistration(this);
				}
				else {
					using(var comps = ListPool<Component>.Get()) {

						childT.GetComponents<Component>(comps);

						bool didRegister = false;
						foreach(var c in comps) {
							foreach(var attr in c.GetType().GetCustomAttributes(false)) {
								var regAttr = attr as ServiceInterfaceAttribute;
								if(regAttr == null) {
									continue;
								}

								new ComponentServiceRegistration(c, regAttr.serviceInterface?? c.GetType()).SetServiceRegistration(this);
							
								didRegister = true;
							}
						}

						if(didRegister) {
							continue;
						}

						// can we auto register?
						if(comps.Count == 2) { 
							// we can auto register if there is only one (non-transform) component attached
							Component service = comps[0] is Transform ? comps[1] : comps[0];
							
							new ComponentServiceRegistration(service).SetServiceRegistration(this);
							
						}
						else {
							Debug.LogWarning("Encountered service object that has multiple non-transform components. Use a ServiceRegistration component!");
						}
					}
				}
			}
		}
		
		virtual protected void RegisterServices() 
		{
			var serviceRegistrations = new List<ServiceRegistration>(m_serviceRegistrationsByType.Values);
			serviceRegistrations.Sort(new SortServiceRegistrationsByGroup());
				
			Services loc = this.services;
				
			foreach(ServiceRegistration r in serviceRegistrations) {
				r.RegisterService(loc);
			}
		}
		
		virtual protected void InitializeServices(Action onCompleteCallback) 
		{
			var serviceRegistrations = new List<ServiceRegistration>(m_serviceRegistrationsByType.Values);
			serviceRegistrations.Sort(new SortServiceRegistrationsByGroup());
			InitNext(serviceRegistrations, onCompleteCallback);
		}

		/// <summary>
		/// Register a service that will be instantiated from a concrete type.
		/// </summary>
		/// <typeparam name="RegistrationInterface">the interface that will be used to locate this service with calls to BeatThat.Service.Services.Locate<RegistrationInterface>().</typeparam>
		/// <typeparam name="ConcreteType">The concrete type of the service. Must implement the given registration interface. 
		/// If this type is a MonoBehaviour, it will be created by unity's Object.Instantiate, otherwise if will be created by its zero-arg constructor
		/// </typeparam>
		public void Register<RegistrationInterface, ConcreteType>()
			where ConcreteType : class, RegistrationInterface, new()
				where RegistrationInterface : class
		{
			new FactoryServiceRegistration<RegistrationInterface, ConcreteType>().SetServiceRegistration(this);
		}

		/// <summary>
		/// Directly register a service to a provided instance
		/// </summary>
		public void Register<RegistrationInterface>(RegistrationInterface service)
		{
			new DirectServiceRegistration<RegistrationInterface>(service).SetServiceRegistration(this);
		}

		public void Register(Type registrationInterface, Type concreteType)
		{
			if(!registrationInterface.IsAssignableFrom(concreteType)) {
				throw new ArgumentException("Registration interface " + registrationInterface.Name
					+ " must be assignable from concrete type " + concreteType.Name);
			}

			new FactoryServiceRegistration(registrationInterface, concreteType).SetServiceRegistration(this);
		}
		
		private void InitNext(List<ServiceRegistration> serviceRegistrations, Action onCompleteCallback)
		{
			if(serviceRegistrations.Count == 0) {
				if(DEBUG_LOAD_SERVICES) {
					Debug.Log("[" + Time.time + "] " + GetType() + " all services init...");
				}

				onCompleteCallback();
			}
			else {
				ServiceRegistration r = serviceRegistrations[0];
				
				if(DEBUG_LOAD_SERVICES) {
					Debug.Log("[" + Time.time + "] " + GetType() + " initing next service " + r + "...");
				}
				
				serviceRegistrations.RemoveAt(0);

				r.InitService(this.services, () => {
					if(DEBUG_LOAD_SERVICES) {
						Debug.Log("[" + Time.time + "] " + GetType() + " service " + r + " init complete...");
					}
					
					InitNext(serviceRegistrations, onCompleteCallback);
				});
			}
		}
		
		/// <summary>
		/// After all services are registered in servicelocator add calls here to services you want started with app startup.
		/// </summary>
		virtual protected void DoStartServices()
		{
		}
		
		virtual protected void UnregisterServices() 
		{
			Services loc = this.services;
			
			foreach(Type registrationInterface in m_serviceRegistrationsByType.Keys) {
				ServiceRegistration r;
				if(m_serviceRegistrationsByType.TryGetValue(registrationInterface, out r)) {
					
					Debug.Log("[" + Time.time + "] " + GetType() + "::UnregisterServices unregistering type '" 
					          + registrationInterface + "'");
					
					try {
						r.UnregisterService(loc);
					}
					catch(Exception e) {
						Debug.LogError(e.Message);
					}
				}
			}
			
			m_serviceRegistrationsByType.Clear();
		}
		
		private readonly Dictionary<Type, ServiceRegistration> m_serviceRegistrationsByType = new Dictionary<Type, ServiceRegistration>();
		private bool m_servicesLoaded;
	}
			
	class SortServiceRegistrationsByGroup : IComparer<ServiceRegistration>
	{
		public int Compare (ServiceRegistration x, ServiceRegistration y)
		{
			return x.registrationGroup - y.registrationGroup;
		}
	}
}