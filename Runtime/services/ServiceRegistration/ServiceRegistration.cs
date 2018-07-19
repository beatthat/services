#pragma warning disable 618
using System;

namespace BeatThat.Service
{
    public interface ServiceRegistration  
	{
		int registrationGroup { get; }
		
		void SetServiceRegistration(ServiceLoader loader);
		
		void RegisterService(Services toLocator);
		
		bool UnregisterService(Services toLocator);
		
		void InitService(Services serviceLocator, System.Action onCompleteCallback);
		
		ServiceType GetService<ServiceType>(Services serviceLocator) where ServiceType : class;
		
		object GetService(Services serviceLocator);

        bool isProxy { get; }

        Type registrationType { get; }
		
	}
}
#pragma warning restore 618

