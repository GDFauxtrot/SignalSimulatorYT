using UnityEngine;

namespace SignalSimulatorYT
{
    class BrowserOutdoorTrigger : MonoBehaviour
    {
        public bool isGoingOutdoors;

        void OnTriggerEnter(Collider other)
        {
            // Player doesn't even use the Player layer? wtf
            // Pick up Player by name and trigger data
            if (other.gameObject.name == "Player")
            {
                BrowserBehavior.Instance.OutdoorTriggerEntered(isGoingOutdoors);
            }
        }
    }
}
