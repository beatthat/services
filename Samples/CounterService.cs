using UnityEngine;

namespace BeatThat.Service.Examples
{
    public interface ICounter
    { 
        int count { get; }

        void Increment();
    }

    [RegisterService]
    public class Counter 
        : MonoBehaviour // Services are not required to be monobehaviours 
                        // but it's convenient to be able to see them in the unity editor
   
        , ICounter      // It's good practice to have narrow interface(s) 
                        // for services and access them via interface.
                        // By default, [RegisterService] will create a proxy registration
                        // for every interface declared directly on the class where you appy the attribute.
    {
        public int count
        {
            get; set;
        }

        public void Increment()
        {
            this.count++;
            Debug.Log("incremented count to " + this.count);
        }
    }
}

