//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using BeatThat.Service;
//
//public class ServiceLocator : MonoBehaviour
//{
//
//	private static bool SELF_LOAD_ON_AWAKE = true;
//
//	public static void DisableSelfLoad()
//	{
//		SELF_LOAD_ON_AWAKE = false;
//	}
//
//	public static ServiceLocator Get 
//	{
//		get {
//			if(INSTANCE == null && (INSTANCE = GameObject.FindObjectOfType<ServiceLocator>()) == null) {
//				INSTANCE = new GameObject("ServiceLocator").AddComponent<ServiceLocator>();
//			}
//			return INSTANCE;
//		}
//	}
//
//	/// <summary>
//	/// Syntactic sugar function. 
//	/// 
//	/// Let's a service user get a service with a cached local copy in one line of code like this:
//	/// 
//	/// var service = ServiceLocator.LocateCached<ServiceType>(ref m_cacheService);
//	/// 
//	/// ...instead of the more verbose
//	/// 
//	/// if(m_cachedService == null) { ...
//	/// 
//	/// </summary>
//	/// <returns>The cached.</returns>
//	/// <param name="cachedRef">Cached reference.</param>
//	/// <typeparam name="ServiceType">The 1st type parameter.</typeparam>
//	public static ServiceType Locate<ServiceType>(ref ServiceType cachedRef) where ServiceType : class
//	{
//		if(cachedRef != null) {
//			return cachedRef;
//		}
//		cachedRef = Locate<ServiceType>();
//		return cachedRef;
//	}
//
//	/// <summary>
//	/// Syntactic sugar version of Require<ServiceType>()
//	/// </summary>
//	public static ServiceType Require<ServiceType>(ref ServiceType cachedRef) where ServiceType : class
//	{
//		if(cachedRef != null) {
//			return cachedRef;
//		}
//		cachedRef = Require<ServiceType>();
//		return cachedRef;
//	}
//
//	/// <summary>
//	/// Locate a service by registration interface and throw an exception if it cannot be located.
//	/// </summary>
//	public static ServiceType Require<ServiceType>() where ServiceType : class
//	{
//		var s = Locate<ServiceType>();
//		if(s == null) {
//			throw new MissingRequiredServiceException(typeof(ServiceType));
//		}
//		return s;
//	}
//
//	/// <summary>
//	/// Locate a service by registration interface.
//	/// </summary>
//	/// <typeparam name="ServiceType">The 1st type parameter.</typeparam>
//	public static ServiceType Locate<ServiceType>() where ServiceType : class
//	{
//		return ServiceLocator.Get.GetService<ServiceType>();
//	}
//
//	/// <summary>
//	/// Perform an action on a service ONLY if the service is available,
//	/// i.e. if the application is quitting, ignore and avoid errors
//	/// </summary>
//	public static void IfAvailable<ServiceType>(System.Action<ServiceType> action) where ServiceType : class
//	{
//		var s = Locate<ServiceType>();
//		if(s == null) {
//			return;
//		}
//
//		action(s);
//	}
//
//
//	public static void SetContext(string ctx)
//	{
//		ServiceLocator.Get.SetLocatorContext(ctx);
//	}
//
//	public void SetLocatorContext(string ctx)
//	{
//		if(string.IsNullOrEmpty(ctx)) {
//			ctx = DEFAULT_CONTEXT_NAME;
//		}
//		else {
//			ctx = ctx.ToLower();
//		}
//
//		Context ctxObj;
//		if(!m_contextsByName.TryGetValue(ctx, out ctxObj)) {
//			ctxObj = new Context(ctx);
//
//			m_contextsByName[ctx] = ctxObj;
//
//			Debug.Log("[" + Time.time + "] " + GetType() 
//			          + "::SetLocatorContext creating new context '" + ctx + "'");
//		}
//
//		this.activeContext = ctx;
//		this.activeContextObj = ctxObj;
//	}
//
//	public string activeContext
//	{
//		get; private set;
//	}
//	
//	public static bool isEnabled
//	{
//		get {
//			return INSTANCE != null;
//		}
//	}
//
//	public static bool exists
//	{
//		get {
//			return INSTANCE != null;
//		}
//	}
//	
//	void Awake() 
//	{
//		SetLocatorContext(this.activeContext);
//		DontDestroyOnLoad(this.gameObject);
//		ServiceLocator.INSTANCE = this;
//		if(SELF_LOAD_ON_AWAKE) {
//			var loader = new GameObject("ServiceLoader").AddComponent<ServiceLoader>();
//			loader.LoadServices(true, () => {
//				Destroy(loader.gameObject);
//			});
//		}
//	}
//	
//	public T AddPersistentComponent<T>(string name) where T : Component
//	{
//		GameObject go = new GameObject(name);
//		go.transform.parent = this.transform;
//		return go.AddComponent<T>();
//	}
//	
//	public void RegisterService<RegistrationInterface>(ServiceRegistration serviceRegistration)
//		where RegistrationInterface : class
//	{
//		RegisterService(serviceRegistration, typeof(RegistrationInterface));
//	}
//	
//	public void RegisterService(ServiceRegistration serviceRegistration, Type registrationInterface)
//	{
//		// force the creation of the registered service and also make sure it's not null
//		object service = serviceRegistration.GetService(this);
//		if(service == null) {
//			throw new NullReferenceException("ServiceRegistration returns null for type "
//				+ registrationInterface.Name + ", registration=" + serviceRegistration);
//		}
//		
//		if(service is Component) {
//			(service as Component).transform.parent = this.transform;
//		}
//
//		GetActiveContext().Set(registrationInterface, serviceRegistration);
//	}
//	
//	public bool UnregisterService<RegistrationInterface>()
//		where RegistrationInterface : class
//	{
//		return UnregisterService(typeof(RegistrationInterface));
//	}
//	
//	public bool UnregisterService(Type registrationInterface)
//	{
//		object service = GetService(registrationInterface);
//		if(service != null && service is Component) {
//			(service as Component).transform.parent = null;
//		}
//
//		return GetActiveContext().Remove(registrationInterface);
//	}
//	
//	public ServiceType GetService<ServiceType>() where ServiceType : class
//	{
//		Type t = typeof(ServiceType);
//		ServiceRegistration registration = null;
//		if(GetActiveContext().TryGet(t, out registration)) {
//			return registration.GetService<ServiceType>(this);
//		}
//		else {
//			Debug.LogWarning("No service registration for type: '" + t.ToString());
//			return null;
//		}
//	}
//	
//	public object GetService(Type registrationInterface)
//	{
//		ServiceRegistration registration = null;
//		if(GetActiveContext().TryGet(registrationInterface, out registration)) {
//			return registration.GetService(this);
//		}
//		else {
//			Debug.LogWarning("No service registration for type: '" + registrationInterface.ToString());
//			return null;
//		}
//	}
//
//	private Context GetActiveContext()
//	{
//		if(this.activeContextObj == null) {
//			SetLocatorContext(DEFAULT_CONTEXT_NAME);
//		}
//		return this.activeContextObj;
//	}
//
//	private Context activeContextObj
//	{
//		get; set;
//	}
//
//	class Context
//	{
//		public Context(string name)
//		{
//			this.name = name;
//		}
//
//		public string name
//		{
//			get; private set;
//		}
//
//		public bool TryGet(Type t, out ServiceRegistration r)
//		{
//			return m_serviceRegistrationsByType.TryGetValue(t, out r);
//		}
//
//		public void Set(Type t, ServiceRegistration r)
//		{
//			m_serviceRegistrationsByType[t] = r;
//		}
//
//		public bool Remove(Type t)
//		{
//			return m_serviceRegistrationsByType.Remove(t);
//		}
//
//		override public string ToString()
//		{
//			return "[Context name='" + this.name + "']";
//		}
//
//		private Dictionary<Type, ServiceRegistration> m_serviceRegistrationsByType = new Dictionary<Type, ServiceRegistration>();
//	}
//
//	private static ServiceLocator INSTANCE;
//	
//	private Dictionary<string, Context> m_contextsByName = new Dictionary<string, Context>();
//	private const string DEFAULT_CONTEXT_NAME = "";
////	private Context m_defaultContext = new Context(DEFAULT_CONTEXT_NAME);
//}
