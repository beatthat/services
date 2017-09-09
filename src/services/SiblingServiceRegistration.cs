using UnityEngine;

namespace BeatThat.Service
{
	/// <summary>
	/// A component that enables a unity/monobehaviour-based service component
	/// to register with the servicelocator using a registration interface rather than the services concrete type, 
	/// e.g. IOSPurchaseService might want to register using the PurchaseService interface.
	/// 
	/// This is only for services that need to be configure on the unity scene 
	/// (say, so that they can have properties configured in the unity editor).
	/// For Monobehaviour services that DO NOT need unity-editor configuration, use FactoryServiceRegistration instead.
	/// </summary>
	public class SiblingServiceRegistration<RegistrationInterface> : MonoBehaviour, ServiceRegistration 
		where RegistrationInterface : class
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

		public void SetServiceRegistration(ServiceLoader loader)
		{
			loader.SetServiceRegistration<RegistrationInterface>(this);
		}
		
		public void RegisterService(Services toLocator)
		{
			toLocator.RegisterService<RegistrationInterface>(this);
		}
		
		public void InitService(Services toLocator, System.Action onCompleteCallback)
		{
			ServiceLoader.DoDefaultInitService(this, toLocator, onCompleteCallback);
		}
		
		public bool UnregisterService(Services toLocator)
		{
			return toLocator.UnregisterService<RegistrationInterface>();
		}
		
		public ServiceType GetService<ServiceType>(Services serviceLocator) where ServiceType : class
		{
			return GetService(serviceLocator) as ServiceType;
		}
		
		public object GetService(Services serviceLocator)
		{
			if(m_service == null) {
				m_service = GetComponent(typeof(RegistrationInterface)) as RegistrationInterface;
			}
			return m_service;
		}
		
		
		private RegistrationInterface m_service;
	}
}
