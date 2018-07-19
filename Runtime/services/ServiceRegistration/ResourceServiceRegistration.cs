#pragma warning disable 618
using System;
using System.Reflection;
using BeatThat.Comments;
using UnityEngine;

namespace BeatThat.Service
{
    /// <summary>
    /// registers a service to the Services by instantiating a GameObject asset loaded from Resources
    /// </summary>
    public class ResourceServiceRegistration : ServiceRegistration
	{
		public ResourceServiceRegistration(Type registrationInterface,
                                           Type concreteType, 
                                           int registrationGroup = Services.REGISTRATION_GROUP_DEFAULT,
                                           ServiceResourceType resourceType = ServiceResourceType.REQUIRED, 
                                           string overrideResourcePath = null)
		{
			this.registrationType = registrationInterface;
			this.concreteType = concreteType;
            SetRegistrationGroup(registrationGroup);
            this.resourceType = resourceType;
            this.overrideResourcePath = overrideResourcePath;
		}


        public bool isProxy { get { return false; } }

        public Type registrationType { get; private set; }
		protected Type concreteType { get; private set; }
        protected ServiceResourceType resourceType { get; private set; }
        protected string overrideResourcePath { get; private set; }

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
            if(this.resourceType == ServiceResourceType.NONE)
            {
                return FactoryCreateService();
            }

            var regInterfaceName = this.registrationType.Name;
            var path = this.overrideResourcePath ?? "Services/" + regInterfaceName;

            var asset = Resources.Load(path, this.registrationType);
            if(asset == null)
            {
                if(this.resourceType == ServiceResourceType.REQUIRED)
                {
                    Debug.LogError("[" + Time.frameCount + "] unable to load required resource for service. Type=" + regInterfaceName + ", path=" + path);
                }
                return FactoryCreateService();
            }

            var service = GameObject.Instantiate(asset);

#if UNITY_EDITOR
            (service as Component).gameObject.name = regInterfaceName;
            var comment = (service as Component).gameObject.AddComponent<Comment>();
            comment.text = "loaded from resource path '" + path + "'";
#endif

            return service;
		}

        private object FactoryCreateService()
        {
            Type ctype = this.concreteType;

            if (typeof(Component).IsAssignableFrom(ctype))
            {
                GameObject go = new GameObject(this.registrationType.Name);
                return go.AddComponent(ctype);
            }
            else
            {
                ConstructorInfo c = ctype.GetConstructor(new Type[] { });
                return c.Invoke(new object[] { });
            }
        }
		
		// object param instead of generic ConcreteType because ios doesn't support virtual generic methods
		virtual protected object ConfigureService(object service)
		{
			return service;
		}

        override public string ToString()
        {
            return "[ResourceServiceRegistration type="
                + this.concreteType.Name + ", interface="
                      + this.registrationType.Name + "]";
        }
		
		private object m_service;
		private int m_registrationGroup = 0;

		
	}
    

}
#pragma warning restore 618

