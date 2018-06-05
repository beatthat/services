using UnityEngine;

namespace BeatThat.Service.Examples
{
    public interface ICounter
    { 
        int count { get; }
    }

    [RegisterService(typeof(ICounter))]
    public class Counter 
        : MonoBehaviour // services are not required to be monobehaviours 
                        //  but it's convenient to be able to see them in the unity editor
   
        , ICounter // it's good practice to have narrow interface(s) for services and access them via interface
    {
        public int count
        {
            get; set;
        }
    }
}

