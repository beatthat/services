using System;

namespace BeatThat.Service
{
	/// <summary>
	/// Marks a class to be autowire registered to Services.
	/// For the autowire registrations to occur, 
	/// generally, this boilerplate should be execute on the first frame of app launch:
	/// 
	/// Services.Init(() => {
	/// 	// if you need to wait for complete start here (services may have some async init behaviour)
	/// });
	/// 
	/// @param serviceInterface (optional) interface used to retrieve the Command from services, 
	/// when left null, registers to the concrete type.
	/// 
	/// 
	/// @param priority (optional/default 0) used in the case that multiple implementations 
	/// 	have autowireservice attributes for the same service interface.
	/// 	Resolves to the implementation with the LOWEST priority value.
	/// 
	/// @param proxyInterfaces (optional) list of alternative interfaces that can be used to locate the service
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class RegisterServiceAttribute : ServiceInterfaceAttribute 
	{
		public RegisterServiceAttribute(Type serviceInterface = null, Type[] proxyInterfaces = null, int priority = 0) : base(serviceInterface, priority) 
		{
			this.proxyInterfaces = proxyInterfaces;
		}

		public Type[] proxyInterfaces { get; private set; }
	}
}
