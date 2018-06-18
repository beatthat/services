using UnityEngine;
using UnityEngine.Events;

namespace BeatThat.Service.Example_DepenendencyInjection_Interfaces
{
    public interface ICounter
    { 
        UnityEvent onUpdated { get; }

        int count { get; }

        void Increment();
    }

    // By default, [RegisterService] will create a proxy registration
    // for every interface declared directly on the class where you appy the attribute.
    // For the class below, it will register the interface ICounter by default.
    [RegisterService]
    public class CounterService_WithInterface 
        : MonoBehaviour // Services are not required to be monobehaviours 
                        // but it's convenient to be able to see them in the unity editor
   
        , ICounter      // It's good practice to have narrow interface(s) 
                        // for services and access them via interface.
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

