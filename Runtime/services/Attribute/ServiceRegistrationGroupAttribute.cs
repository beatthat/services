using System;

namespace BeatThat.Service
{

    [AttributeUsage(AttributeTargets.All)]
	public class ServiceRegistrationGroupAttribute : System.Attribute 
	{
	  	public ServiceRegistrationGroupAttribute(int registrationGroup)  
		{
			this.registrationGroup = registrationGroup;
		}
		
		public int registrationGroup
		{
			get; private set;
		}
	}
}

