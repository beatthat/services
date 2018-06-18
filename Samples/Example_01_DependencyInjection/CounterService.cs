using UnityEngine;
using UnityEngine.Events;

namespace BeatThat.Service.Example_DepenendencyInjection
{
    // Add the [RegisterService] attribute 
    // to classes that should be managed as singleton services.
    // Registered services can be set as properties on other classes
    // via dependency injection
    [RegisterService]
    public class CounterService 

        : MonoBehaviour // Services are not required to be monobehaviours 
                        // but it's convenient to be able to see them in the unity editor
   
    {
        public UnityEvent onUpdated { get { return m_onUpdated ?? (m_onUpdated = new UnityEvent()); } }
        private UnityEvent m_onUpdated;

        public int count
        {
            get; private set;
        }

        public void Increment()
        {
            this.count++;
            this.onUpdated.Invoke();
        }
    }
}

