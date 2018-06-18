using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.Service.Example_DepenendencyInjection
{
    [RequireComponent(typeof(Text))]
    public class CounterDisplay : DependencyInjectedBehaviour
    {
        public Text m_text;

        /**
         * Dependency injection tries to locate and set properties marked with the [Inject] attribute
         */
        [Inject] ICounter counter;

        /**
         * Base class DependencyInjectedBehaviour implements DependencyInjectionEventHandler,
         * and so, we get a callback when dependency injection is complete
         */ 
        override public void OnDidInjectDependencies()
        {
            m_text = GetComponent<Text>();
            
            // since dependency injection is complete, the counter property should be set now
            this.counter.onUpdated.AddListener(this.UpdateDisplay);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            m_text.text = this.counter.count.ToString();
        }


    }
}