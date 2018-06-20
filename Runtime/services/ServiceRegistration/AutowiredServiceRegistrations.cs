using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatThat.TypeExts;

namespace BeatThat.Service
{
    public static class AutowiredServiceRegistrations 
	{
		public static void GetWirings(ICollection<ServiceRegistrationInfo> wirings)
		{
			EnsureWiringsLoaded();

			foreach(var e in m_wiringsByInterface) {
                var regInterface = e.Key;
                var regImpl = e.Value[0].implementationType;

                wirings.Add(
                    new ServiceRegistrationInfo(e.Key, 
                    e.Value[0].implementationType, 
                    e.Value[0].resourceServiceAttr != null ? e.Value[0].resourceServiceAttr.serviceResourceType: ServiceResourceType.NONE,
                    e.Value[0].resourceServiceAttr != null ? e.Value[0].resourceServiceAttr.overrideResourcePath : null
                    ));
			}
		}

		/// <summary>
		/// Get the set of proxy interfaces for a given registration interface,
		/// e.g. there might be a regisration interface UserService with proxy interface EnityService<User>
		/// </summary>
		/// <returns><c>true</c>, if the registration interface exists and has proxy interfaces, <c>false</c> otherwise.</returns>
		/// <param name="registrationInterface">the registration interface.</param>
		/// <param name="proxyInterfaces">Any proxy interfaces found are added to this result list</param>
        public static bool GetProxyInterfaces(Type serviceType, Type registrationInterface, List<Type> proxyInterfaces)
		{
			AttributesAndImplementationType[] wirings;
			if(!m_wiringsByInterface.TryGetValue(registrationInterface, out wirings)) {
				return false;
			}

			if(wirings == null || wirings.Length == 0) {
				return false;
			}

			var attr = wirings[0].registerServiceAttr;
			if(attr.proxyInterfaces != null && attr.proxyInterfaces.Length > 0) {
                proxyInterfaces.AddRange(attr.proxyInterfaces);
			}

            switch (attr.interfaceRegistrationPolicy)
            {
                case InterfaceRegistrationPolicy.RegisterInterfacesDeclaredOnType:
                    GetInterfaces(serviceType, false, proxyInterfaces);
                    break;
                case InterfaceRegistrationPolicy.RegisterInterfacesDeclaredOnTypeAndParents:
                    GetInterfaces(serviceType, true, proxyInterfaces);
                    break;
                case InterfaceRegistrationPolicy.RegisterInterfacesDeclaredOnTypeIfNoProxyInterfaces:
                    if (proxyInterfaces.Count == 0)
                    {
                        GetInterfaces(serviceType, false, proxyInterfaces);
                    }
                    break;
            }

            return proxyInterfaces.Count > 0;
		}

        private static void GetInterfaces(this Type type, bool includeInherited, List<Type> interfaces)
        {
            if (includeInherited || type.BaseType == null)
            {
                interfaces.AddRange(type.GetInterfaces());
            }
            else
            {
                interfaces.AddRange(type.GetInterfaces().Except(type.BaseType.GetInterfaces()));
            }
        }


		public static bool TryGetImplForInterface(Type intf, out Type impl)
		{
			EnsureWiringsLoaded();

			AttributesAndImplementationType[] wirings;
			if(m_wiringsByInterface.TryGetValue(intf, out wirings)) {
				impl = wirings[0].implementationType;
				return true;
			}

			impl = null;
			return false;
		}

		private static void EnsureWiringsLoaded()
		{
			if(!m_hasLoadedWirings) {
				LoadWiringsByInterface();
			}
		}

		private static void LoadWiringsByInterface()
		{
			m_hasLoadedWirings = true;

			var wiringsByInterface = new Dictionary<Type, List<AttributesAndImplementationType>>();

			foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type t in a.GetTypes()) {
                    var wiringAttr = t.GetCustomAttribute<RegisterServiceAttribute>();
                    if(wiringAttr == null)
                    {
                        continue;
                    }

                    var resourceAttr = t.GetCustomAttribute<ResourceServiceAttribute>();

					// if the RegisterService attribute specifies a service interface, register to that, 
					// if not, register to the concrete service type
					var registrationType = wiringAttr.serviceInterface?? t;
					
					List<AttributesAndImplementationType> wirings;
					if(!wiringsByInterface.TryGetValue(registrationType, out wirings)) {
						wirings = new List<AttributesAndImplementationType>(1);
						wiringsByInterface[registrationType] = wirings;
					}

					wirings.Add(new AttributesAndImplementationType(t, wiringAttr, resourceAttr));
	
				}
			}
				
			m_wiringsByInterface.Clear();

			foreach(var e in wiringsByInterface) {
				var wirings = e.Value.ToArray();

				if(wirings.Length > 1) {
					Array.Sort(wirings, SORT_BY_PRIORITY); // this should be a Comparison/lamda expression, but somehow refuses to sort some (2-item?) arrays using that method.
				}

				m_wiringsByInterface[e.Key] = wirings;
			}
		}

		private static IComparer<AttributesAndImplementationType> SORT_BY_PRIORITY = new PriorityComparer();

		class PriorityComparer : IComparer<AttributesAndImplementationType>
		{
			#region IComparer implementation
			public int Compare (AttributesAndImplementationType x, AttributesAndImplementationType y)
			{
				return y.registerServiceAttr.priority - x.registerServiceAttr.priority;
			}
			#endregion
		}

		struct AttributesAndImplementationType
		{
			public AttributesAndImplementationType(Type implType, RegisterServiceAttribute regSrv, ResourceServiceAttribute rsrcSrv)
			{
				this.implementationType = implType;
                this.registerServiceAttr = regSrv;
                this.resourceServiceAttr = rsrcSrv;
            }

			public RegisterServiceAttribute registerServiceAttr;
            public ResourceServiceAttribute resourceServiceAttr;
			public Type implementationType;
		}

		private static bool m_hasLoadedWirings;
		private static readonly Dictionary<Type, AttributesAndImplementationType[]> m_wiringsByInterface = new Dictionary<Type, AttributesAndImplementationType[]>();
	}

}


