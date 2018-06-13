#pragma warning disable 618
namespace BeatThat.Service
{
    /// <summary>
    /// Same as an AutoInitService (implementing this interface causes ServiceLoader to call InitService at startup),
    /// except AsyncInitServices assume stop service loader while some async-init op completes.
    /// 
    /// </summary>
    public interface AsyncInitService
	{
		void InitServiceAsync(Services serviceLocator, System.Action onComplete);
	}
}
#pragma warning restore 618
