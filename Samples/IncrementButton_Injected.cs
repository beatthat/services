using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.Service.Examples
{
    [RequireComponent(typeof(Button))]
    public class IncrementButton_Injected : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(this.OnClick);
            DependencyInjection.InjectDependencies(this);
        }

        public void OnClick()
        {
            this.counter.Increment();
        }

        [Inject] ICounter counter;

    }
}