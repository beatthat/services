#pragma warning disable 618
using System;

namespace BeatThat.Service
{
    public class ProxyServiceRegistration : ServiceRegistration
	{
		public ProxyServiceRegistration(
            Type registrationInterface,
            Type concreteType,
            int registrationGroup = Services.REGISTRATION_GROUP_DEFAULT
        )
		{
			this.registrationType = registrationInterface;
			this.proxyInterface = concreteType;
            SetRegistrationGroup(registrationGroup);
		}

        public Type registrationType { get; private set; }
		protected Type proxyInterface { get; private set; }

		public int registrationGroup { get { return m_registrationGroup; } }
		
		public ServiceRegistration SetRegistrationGroup(int registrationGroup)
		{
			m_registrationGroup = registrationGroup;
			return this;
		}
			
		public void SetServiceRegistration(ServiceLoader loader)
		{
			loader.SetServiceRegistration(this, this.registrationType);
		}
			
		public void RegisterService(Services toLocator)
		{
			toLocator.RegisterService(this, this.registrationType);
		}
			
		virtual public void InitService(Services serviceLocator, System.Action onCompleteCallback)
		{
			// generally do not auto init the proxied service, because the service itself will autoinit	
			onCompleteCallback();
		}
		
		public bool UnregisterService(Services toLocator)
		{
			return toLocator.UnregisterService(this.registrationType);
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

        public bool isProxy { get { return true; } }

        override public string ToString()
        {
            return "[ProxyServiceRegistration type="
                + this.registrationType.Name + ", proxies="
                      + this.proxyInterface.Name + "]";
        }

		
		private object m_service;
		private int m_registrationGroup = 1;
	}
}
#pragma warning restore 618

