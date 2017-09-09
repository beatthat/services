using System;

namespace BeatThat.Service
{
	public class MissingRequiredServiceException : System.Exception 
	{
		public MissingRequiredServiceException(Type type) : base("No service regisration for required type '" + type.Name + "'")
		{
				this.type = type;
		}

		public Type type { get; private set; }
	}
}
