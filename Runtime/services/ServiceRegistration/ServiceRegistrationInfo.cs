#pragma warning disable 618
using System;

namespace BeatThat.Service
{
    public enum ServiceResourceType {  NONE = 0, REQUIRED = 1, PREFERRED = 2 }

	public struct ServiceRegistrationInfo  
	{

        public ServiceRegistrationInfo(
            Type intf, 
            Type impl, 
            int registrationGroup = Services.REGISTRATION_GROUP_DEFAULT,
            ServiceResourceType resourceType = ServiceResourceType.NONE, 
            string overrideResourcePath = null)
		{
			this.interfaceType = intf;
			this.implType = impl;
            this.registrationGroup = registrationGroup;
            this.resourceType = resourceType;
            this.overrideResourcePath = overrideResourcePath;
		}

        /// <summary>
        /// Implementation type used to create the Service. Must implement the type of property interfaceType.
        /// </summary>
        public Type implType { get; private set; }

        /// <summary>
        /// The interface type for a service. This will be the primary type used to retrieve the service, e.g. <code>Service.Require<SomeInterfaceType>()</InterfaceType></code>
        /// </summary>
		public Type interfaceType { get; private set; }

        /// <summary>
        /// Registration groups provide a way to provide control over
        /// the order in which registered services are instantiated and
        /// receive their InitService message
        /// (for services that implemenent AutoInitService).
        /// </summary>
        public int registrationGroup { get; private set; }

        /// <summary>
        /// Sometimes you want to have a service whose GameObject has multiple components and/or custom configuration.
        /// For these cases, you can tell the ServiceLoader to load the service from Resources by adding a [ResourceService] attribute to the implementation.
        /// </summary>
        public ServiceResourceType resourceType { get; private set; }

        /// <summary>
        /// Generally services loaded as resources use the interface name as the path, e.g. "Services/${interfaceName}"
        /// You can override the path by using this property
        /// </summary>
        public string overrideResourcePath { get; private set; }
       
	}
}
#pragma warning restore 618

