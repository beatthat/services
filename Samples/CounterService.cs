using UnityEngine;

namespace BeatThat.Service.Examples
{
    public interface ICounter
    { 
        int count { get; }

        void Increment();
    }

    [RegisterService(typeof(ICounter))]
    public class Counter 
        : MonoBehaviour // Services are not required to be monobehaviours 
                        // but it's convenient to be able to see them in the unity editor
   
        , ICounter      // It's good practice to have narrow interface(s) 
                        // for services and access them via interface
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

