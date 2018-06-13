#pragma warning disable 618
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
		
	}
}
#pragma warning restore 618

