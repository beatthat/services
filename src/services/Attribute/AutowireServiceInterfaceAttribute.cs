using System;

namespace BeatThat.Service
{
	/// <summary>
	/// Apply this attribute to concrete implementations of a service interface
	///  and that will become the default impl returned by the Services.
	/// @param serviceInterface the interface under which the service will be registered
	/// @param proxyInterfaces (optional) list of alternative interfaces that can be used to locate the service
	/// @param priority (optional/default 0) used in the case that multiple implementations 
	/// 	have autowireservice attributes for the same service interface.
	/// 	Resolves to the implementation with the lowest priority value.
	/// </summary>
	[System.Obsolete("use RegisterService attribute instead")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class AutowireServiceInterfaceAttribute : RegisterServiceAttribute 
	{
		public AutowireServiceInterfaceAttribute(Type serviceInterface, Type[] proxyInterfaces = null, int priority = 0) : base(serviceInterface, proxyInterfaces, priority) {}
	}
}
