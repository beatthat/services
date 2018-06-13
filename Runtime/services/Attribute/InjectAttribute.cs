using System;

namespace BeatThat.Service
{
    /// <summary>
    /// Add this to a field (or a property) where the type matches an interface 
    /// registered with Services to request the dependency injected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class InjectAttribute : Attribute
	{
	}
}

