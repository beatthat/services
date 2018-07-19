#pragma warning disable 618
using System;
using System.Reflection;
using UnityEngine;
namespace BeatThat.Service
{
    /// <summary>
    /// registers a service to the Services by instantiating the given concrete type
    /// </summary>
    public class FactoryServiceRegistration : ServiceRegistration
	{
		public FactoryServiceRegistration(
            Type registrationInterface, 
            Type concreteType, 
            int registrationGroup = Services.REGISTRATION_GROUP_DEFAULT)
		{
			this.registrationType = registrationInterface;
			this.concreteType = concreteType;
            SetRegistrationGroup(registrationGroup);
		}

        public Type registrationType { get; private set; }
		protected Type concreteType { get; private set; }

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
			ServiceLoader.DoDefaultInitService(this, serviceLocator, onCompleteCallback);
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
			if(m_service == null) {
				m_service = NewInstanceOfService();
			}
			return m_service;
		}
		
		protected object NewInstanceOfService()
		{
			return ConfigureService(CreateService());
		}
		
		// returns object instead of generic ConcreteType because ios doesn't support virtual generic methods
		virtual protected object CreateService()
		{
			Type ctype = this.concreteType;
			
			if(typeof(Component).IsAssignableFrom(ctype))
			{
				GameObject go = new GameObject(this.registrationType.Name);
				return go.AddComponent(ctype);
			}
			else
			{
				ConstructorInfo c = ctype.GetConstructor(new Type[]{});
				return c.Invoke(new object[]{});
			}
		}
		
		// object param instead of generic ConcreteType because ios doesn't support virtual generic methods
		virtual protected object ConfigureService(object service)
		{
			return service;
		}
		
        override public string ToString()
        {
            return "[FactoryServiceRegistation type=" 
                + this.concreteType.Name + ", interface=" 
                      + this.registrationType.Name + "]";
        }

        public bool isProxy { get { return false; } }

		private object m_service;
		private int m_registrationGroup = 0;

		
	}

	/// <summary>
	/// registers a service to the Services where the registration interface is the same as the concrete type of the service, i.e. there is no interface
	/// </summary>
	public class FactoryServiceRegistration<ConcreteType> : FactoryServiceRegistration
		where ConcreteType : class, new() 
	{
		public FactoryServiceRegistration() : base(typeof(ConcreteType), typeof(ConcreteType)) {}
	}


	public class FactoryServiceRegistration<RegistrationInterface, ConcreteType> : FactoryServiceRegistration
		where ConcreteType : class, RegistrationInterface, new()
		where RegistrationInterface : class
	{
		public FactoryServiceRegistration() : base(typeof(RegistrationInterface), typeof(ConcreteType)) {}
	}

}
#pragma warning restore 618

