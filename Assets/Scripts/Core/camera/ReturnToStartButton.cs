using UnityEngine;

namespace Core.camera
{
    /*
     * UI hook for a "return to start" button.
     * The button does not move the player itself; it asks the existing camera navigation system
     * to switch back to the configured start spot.
     */
    public class ReturnToStartButton : MonoBehaviour
    {
        [SerializeField] private SpotManager spotManager;
        [SerializeField] private CameraTransitionManager transitionManager;

        private void Awake()
        {
            if (spotManager == null)
                spotManager = FindFirstObjectByType<SpotManager>();

            if (transitionManager == null)
                transitionManager = FindFirstObjectByType<CameraTransitionManager>();
        }

        public void ReturnToStart()
        {
            if (spotManager == null)
            {
                Debug.LogError($"{nameof(ReturnToStartButton)} has no SpotManager assigned.", this);
                return;
            }

            if (transitionManager == null)
            {
                Debug.LogError($"{nameof(ReturnToStartButton)} has no CameraTransitionManager assigned.", this);
                return;
            }

            string startSpotId = spotManager.GetStartSpotId();
            if (string.IsNullOrWhiteSpace(startSpotId))
            {
                Debug.LogError($"{nameof(ReturnToStartButton)} cannot return because no start spot id was found.", this);
                return;
            }

            transitionManager.PlayRoute(null, startSpotId);
        }
    }
}
