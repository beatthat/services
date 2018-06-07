using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.Service.Examples
{
    [RequireComponent(typeof(Text))]
    public class CounterDisplay : MonoBehaviour
    {
        public Text m_text;

        void Start()
        {
            DependencyInjection.InjectDependencies(this);
            m_text = GetComponent<Text>();
        }

        void Update()
        {
            m_text.text = this.counter.count.ToString();
        }

        [Inject] ICounter counter;

    }
}