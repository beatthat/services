namespace BeatThat.Service
{
    /// <summary>
    /// For components that need to implement behviour, say, after dependencies have been injected.
    /// </summary>
    public interface DependencyInjectionEventHandler 
	{
        /// <summary>
        /// Called by DependencyInjection.InjectDependencies when injection
        /// is postponed because services have not yet init.
        /// 
        /// DependencyInjection will queue the target component and 
        /// retry injection when the services are ready.
        /// </summary>
        void OnDependencyInjectionWaitingForServicesReady();

        /// <summary>
        /// Called immediatedly before dependencies are injected
        /// </summary>
        void OnWillInjectDependencies();

        /// <summary>
        /// Called immediatedly after dependencies are injected
        /// </summary>
        void OnDidInjectDependencies();


    }
}
