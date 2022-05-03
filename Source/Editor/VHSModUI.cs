using UnityEngine;
using UnityEngine.UI;

namespace SignalSimulatorYT.Shared
{
    public class VHSModUI : MonoBehaviour
    {
        public static VHSModUI Instance;

        // Publicly-accessible fields assigned by Inspector
        public Transform PresetListContent;
        public GameObject ExamplePresetListItem;

        public Button PresetSaveButton;
        public Button PresetLoadButton;

        public InputField LinkInputField;
        public Button OpenLinkButton;

        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
        }
    }
}
