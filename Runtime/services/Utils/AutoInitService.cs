#pragma warning disable 618
namespace BeatThat.Service
{
	/// <summary>
	/// Declare AutoInitService on a service to get an InitService callback from the ServiceLoader.
	/// This is different from having a Start method mainly in that InitService is called when all services have been registered.
	/// </summary>
	public interface AutoInitService
	{
		void InitService(Services services);
		
	}
}
#pragma warning restore 618
