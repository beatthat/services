using System;

namespace BeatThat.Service
{
    /// <summary>
    /// Sometimes you want to have a service whose GameObject has multiple components and/or custom configuration.
    /// For these cases, you can tell the ServiceLoader to load the service from Resources by adding a [ResourceService] attribute to the implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ResourceServiceAttribute : Attribute
    {
		public ResourceServiceAttribute(ServiceResourceType serviceResourceType = ServiceResourceType.REQUIRED, string overrideResourcePath = null)
		{
			this.serviceResourceType = serviceResourceType;
            this.overrideResourcePath = overrideResourcePath;
		}

        /// <summary>
        /// If REQUIRED, then will throw an exception when if the service cannot be loaded from resources.
        /// If PREFERRED, will try first to load from resources then fallback on creating a new instance if resource load fails.
        /// </summary>
		public ServiceResourceType serviceResourceType { get; private set; }

        /// <summary>
        /// The default path will be "Services/${serviceInterface}"
        /// </summary>
        public string overrideResourcePath { get; private set; }
	}
}
