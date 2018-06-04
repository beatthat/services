#pragma warning disable 618
using UnityEngine;
using System;

namespace BeatThat.Service
{
	public class ComponentServiceRegistration : ServiceRegistration
	{ 
		public ComponentServiceRegistration(Component service) : this(service, service.GetType()) {}

		public ComponentServiceRegistration(Component srv, Type regInterface)
		{
			this.service = srv;
			this.registrationInterface = regInterface;
		}
		
		public int registrationGroup { get; private set; }
		
		public ServiceRegistration SetRegistrationGroup(int r)
		{
			this.registrationGroup = r;
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
			ServiceLoader.DoDefaultInitService(this, serviceLocator, onCompleteCallback);
		}
		
		public bool UnregisterService(Services toLocator)
		{
			return toLocator.UnregisterService(this.registrationInterface);
		}
		
		public ServiceType GetService<ServiceType>(Services serviceLocator)
			where ServiceType : class
		{
			var s = this.service as ServiceType;
			if(s != null) {
				return s;
			}

			if(!m_service.isValid) {
				Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + "<" + this.registrationInterface.Name + ">"
					+ "::GetService<" + typeof(ServiceType).Name + "> registered service component is in valid (probably destroyed)");
			}

			return null;
		}
		
		public object GetService (Services serviceLocator)
		{
			return this.service;
		}

		protected Type registrationInterface { get; private set; }
		protected Component service { get { return m_service.value; } private set { m_service = new SafeRef<Component>(value); } } 
		private SafeRef<Component> m_service;

		
	}
}
#pragma warning restore 618

