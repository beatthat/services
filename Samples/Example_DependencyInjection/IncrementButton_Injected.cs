using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.Service.Example_DepenendencyInjection
{
    [RequireComponent(typeof(Button))]
    public class IncrementButton_Injected : MonoBehaviour
    {
        /**
         * Dependency injection tries to locate and set properties marked with the [Inject] attribute
         */
        [Inject] ICounter counter;

        void Start()
        {
            /**
            * Something needs to call DependencyInjection.InjectDependencies.
            * 
            * One option is to call it in MonoBehaviour::Start...
            */
            DependencyInjection.InjectDependencies(this);

            GetComponent<Button>().onClick.AddListener(this.OnClick);
        }

        public void OnClick()
        {
            this.counter.Increment();
        }


    }
}