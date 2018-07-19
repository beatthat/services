using System;

namespace BeatThat.Service
{

    [AttributeUsage(AttributeTargets.All)]
	public class RegistrationGroupAttribute : System.Attribute 
	{
	  	public RegistrationGroupAttribute(int registrationGroup)  
		{
			this.registrationGroup = registrationGroup;
		}
		
		public int registrationGroup
		{
			get; private set;
		}
	}
}

