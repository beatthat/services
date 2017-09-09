using System;

namespace BeatThat.Service
{
	/// <summary>
	/// Add this to a service that may be set up as a scene object to make it discoverable/registerable
	/// to its (parent) Services, e.g.
	/// 
	/// <code>
	/// [ServiceInterface(typeof(ISomeService))] // this tells the Services component to register this object for interface ISomeService
	/// public class SceneObjectService : ISomeService {} // this will live in a game object with config and maybe children
	/// </code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class ServiceInterfaceAttribute : Attribute
	{
		public ServiceInterfaceAttribute(Type serviceInterface = null, int priority = 1)  
		{
			this.serviceInterface = serviceInterface;
			this.priority = priority;
		}

		/// <summary>
		/// Service interface that will be wired with the class marked by the attribute
		/// </summary>
		public Type serviceInterface { get; private set; }

		/// <summary>
		/// For the case where multiple classes declare themselves a wiring the same interface,
		/// the class with the highest priority value will be the one used.
		/// </summary>
		public int priority { get; private set; }
	}
}
