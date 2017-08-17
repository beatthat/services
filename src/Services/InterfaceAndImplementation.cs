using System;

namespace BeatThat.Service
{
	public struct InterfaceAndImplementation  
	{
		public InterfaceAndImplementation(Type intf, Type impl)
		{
			this.interfaceType = intf;
			this.implType = impl;
		}
		
		public Type implType { get; private set; }

		public Type interfaceType { get; private set; }
	}
}
