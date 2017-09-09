using System;

namespace BeatThat.Service
{
	public class ProxyServiceRegistration : ServiceRegistration
	{
		public ProxyServiceRegistration(Type registrationInterface, Type concreteType)
		{
			this.registrationInterface = registrationInterface;
			this.proxyInterface = concreteType;
		}

		protected Type registrationInterface { get; private set; }
		protected Type proxyInterface { get; private set; }

		public int registrationGroup { get { return m_registrationGroup; } }
		
		public ServiceRegistration SetRegistrationGroup(int registrationGroup)
		{
			m_registrationGroup = registrationGroup;
			return this;
		}
			
		public void SetServiceRegistration(ServiceLoader loader)
		{
			loader.SetServiceRegistration(this, this.registrationInterface);
		}
			
		public void RegisterService(Services toLocator)
		{
			toLocator.RegisterService(this, this.registrationInterface);
		}
			
		virtual public void InitService(Services serviceLocator, System.Action onCompleteCallback)
		{
			// generally do not auto init the prxied service, because the service itself will autoinit	
			onCompleteCallback();
		}
		
		public bool UnregisterService(Services toLocator)
		{
			return toLocator.UnregisterService(this.registrationInterface);
		}
			
		public ServiceType GetService<ServiceType>(Services serviceLocator)
			where ServiceType : class
		{
			return GetService(serviceLocator) as ServiceType;
		}
			
		public object GetService(Services serviceLocator)
		{
			return m_service?? (m_service = serviceLocator.GetService(this.proxyInterface));
		}
		
		private object m_service;
		private int m_registrationGroup = 1;
	}
}
