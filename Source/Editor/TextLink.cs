using UnityEngine;
using UnityEngine.UI;

namespace SignalSimulatorYT.Shared
{
    public class TextLink : MonoBehaviour
    {
        public string url;

        public Button button;

        void Awake()
        {
            if (button)
            {
                button.onClick.AddListener(OnButtonPressed);
            }
        }

        private void OnButtonPressed()
        {
            Application.OpenURL(url);
        }
    }
}
