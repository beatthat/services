using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeatThat.Service
{
	public static class AutowiredServiceRegistrations 
	{
		public static void GetWirings(ICollection<InterfaceAndImplementation> wirings)
		{
			EnsureWiringsLoaded();

			foreach(var e in m_wiringsByInterface) {
				wirings.Add(new InterfaceAndImplementation(e.Key, e.Value[0].implementationType));
			}
		}

		/// <summary>
		/// Get the set of proxy interfaces for a given registration interface,
		/// e.g. there might be a regisration interface UserService with proxy interface EnityService<User>
		/// </summary>
		/// <returns><c>true</c>, if the registration interface exists and has proxy interfaces, <c>false</c> otherwise.</returns>
		/// <param name="registrationInterface">the registration interface.</param>
		/// <param name="proxyInterfaces">Any proxy interfaces found are added to this result list</param>
		public static bool GetProxyInterfaces(Type registrationInterface, List<Type> proxyInterfaces)
		{
			AttributeAndImplementationType[] wirings;
			if(!m_wiringsByInterface.TryGetValue(registrationInterface, out wirings)) {
				return false;
			}

			if(wirings == null || wirings.Length == 0) {
				return false;
			}

			var attr = wirings[0].attribute;
			if(attr.proxyInterfaces == null || attr.proxyInterfaces.Length == 0) {
				return false;
			}

			proxyInterfaces.AddRange(attr.proxyInterfaces);
			return true;
		}

		public static bool TryGetImplForInterface(Type intf, out Type impl)
		{
			EnsureWiringsLoaded();

			AttributeAndImplementationType[] wirings;
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

			var wiringsByInterface = new Dictionary<Type, List<AttributeAndImplementationType>>();

			foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type t in a.GetTypes()) {
					foreach(var attr in t.GetCustomAttributes(false)) {
						var wiringAttr = attr as RegisterServiceAttribute;
						if(wiringAttr == null) {
							continue;
						}

						// if the RegisterService attribute specifies a service interface, register to that, 
						// if not, register to the concrete service type
						var registrationType = wiringAttr.serviceInterface?? t;
					
						List<AttributeAndImplementationType> wirings;
						if(!wiringsByInterface.TryGetValue(registrationType, out wirings)) {
							wirings = new List<AttributeAndImplementationType>();
							wiringsByInterface[registrationType] = wirings;
						}

						wirings.Add(new AttributeAndImplementationType(wiringAttr, t));
					}
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

		private static IComparer<AttributeAndImplementationType> SORT_BY_PRIORITY = new PriorityComparer();

		class PriorityComparer : IComparer<AttributeAndImplementationType>
		{
			#region IComparer implementation
			public int Compare (AttributeAndImplementationType x, AttributeAndImplementationType y)
			{
				return y.attribute.priority - x.attribute.priority;
			}
			#endregion
		}

		struct AttributeAndImplementationType
		{
			public AttributeAndImplementationType(RegisterServiceAttribute a, Type t)
			{
				this.attribute = a;
				this.implementationType = t;
			}

			public RegisterServiceAttribute attribute;
			public Type implementationType;
		}

		private static bool m_hasLoadedWirings;
		private static readonly Dictionary<Type, AttributeAndImplementationType[]> m_wiringsByInterface = new Dictionary<Type, AttributeAndImplementationType[]>();
	}
}
