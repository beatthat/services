#pragma warning disable 618
using UnityEngine;
using BeatThat.Service;
namespace BeatThat.Service
{
	public abstract class MBServiceRegistration : MonoBehaviour, ServiceRegistration
	{ 
		public int registrationGroup
		{
			get {
				foreach(object attr in GetType().GetCustomAttributes(true)) {
		            if(attr is ServiceRegistrationGroupAttribute) {
						return (attr as ServiceRegistrationGroupAttribute).registrationGroup;
					}
		        }
				return m_registrationGroup;
			}
		}

		private int m_registrationGroup = 0;

		public ServiceRegistration SetRegistrationGroup(int registrationGroup)
		{
			m_registrationGroup = registrationGroup;
			return this;
		}
		
		public abstract void SetServiceRegistration(ServiceLoader loader);
		
		public abstract void RegisterService(Services toLocator);
		
		public abstract void InitService(Services toLocator, System.Action onCompleteCallback);
		
		public abstract bool UnregisterService(Services toLocator);
		
		public abstract ServiceType GetService<ServiceType>(Services serviceLocator) where ServiceType : class;
		
		public abstract object GetService(Services serviceLocator);

	}
}
#pragma warning restore 618
