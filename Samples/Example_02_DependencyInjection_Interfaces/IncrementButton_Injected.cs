using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.Service.Example_DepenendencyInjection_Interfaces
{
    [RequireComponent(typeof(Button))]
    public class IncrementButton_Injected : MonoBehaviour
    {
        // a service can be injected by any registered interface
        [Inject] ICounter counter;

        void Start()
        {
            //Something needs to call DependencyInjection.InjectDependencies.
            //
            //One option is to call it in MonoBehaviour::Start...
            DependencyInjection.InjectDependencies(this);

            GetComponent<Button>().onClick.AddListener(this.OnClick);
        }

        public void OnClick()
        {
            this.counter.Increment();
        }


    }
}