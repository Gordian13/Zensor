using UnityEngine;

namespace Core.camera
{
    /**
     * This Class is a placeholder for the current selected Spot.
     */
    public class SpotManager : MonoBehaviour
    {
        /** The spot the game starts at, use this or startSpotId. */
        [SerializeField] private CameraSpot _startSpot;
        /** Id of the spot the game starts at, use this or _startSpot. */
        [SerializeField] private string startSpotId;
        /** Used to find the start spot by startSpotId. */
        [SerializeField] private CameraSpotRegistry registry;

        /** The spot the camera is at right now. */
        private CameraSpot CurrentSpot;

        /** Sets the start spot as current spot. */
        private void Awake()
        {
            CurrentSpot = _startSpot;

            if (registry == null)
                Debug.LogError($"{nameof(SpotManager)} has no CameraSpotRegistry assigned.", this);
        }

        /**
         * Finds the start spot and turns its look control on.
         * Logs an error if both or none of _startSpot and startSpotId are set.
         */
        private void Start()
        {
            if (_startSpot != null && !string.IsNullOrWhiteSpace(startSpotId))
                Debug.LogError($"{nameof(SpotManager)} has both Start Spot and Start Spot Id assigned. Use only one.", this);

            ResolveStartSpot();

            if (CurrentSpot == null)
                Debug.LogError($"{nameof(SpotManager)} has no current spot. Assign Start Spot or Start Spot Id.", this);
            else
                CurrentSpot.SetLookControlActive(true);
        }

        /**
         * Changes the current spot.
         * The old spot gets its look control turned off and reset, the new one gets it turned on.
         *
         * @param spot The new current spot.
         */
        public void SetCurrentSpot(CameraSpot spot)
        {
            if (spot == null)
            {
                Debug.LogError($"{nameof(SpotManager)} cannot set current spot to null.", this);
                return;
            }

            if (CurrentSpot != null)
            {
                CurrentSpot.SetLookControlActive(false);
                CurrentSpot.ResetLook();
            }

            CurrentSpot = spot;
            CurrentSpot.SetLookControlActive(true);
        }

        /** @return The current spot. */
        public CameraSpot GetCurrentSpot()
        {
            ResolveStartSpot();
            return CurrentSpot;
        }

        /** @return The id of the current spot, or an empty string if there is none. */
        public string GetCurrentSpotId()
        {
            if (CurrentSpot == null)
            {
                Debug.LogError($"{nameof(SpotManager)} has no current spot.", this);
                return string.Empty;
            }

            return CurrentSpot.GetSpotId();
        }

        /**
         * @brief Returns the navigation ID used by ReturnToStartButton.
         * @return The configured startSpotId, otherwise the ID of the assigned start spot.
         * Logs an error and returns an empty string if neither is configured.
         * This lookup does not change the current spot or start a transition.
         */
        public string GetStartSpotId()
        {
            if (!string.IsNullOrWhiteSpace(startSpotId))
                return startSpotId;

            if (_startSpot != null)
                return _startSpot.GetSpotId();

            Debug.LogError($"{nameof(SpotManager)} has no start spot assigned.", this);
            return string.Empty;
        }

        /**
         * Checks if the given spot is the current spot.
         *
         * @param spot The spot to check.
         * @return True if it is the current spot.
         */
        public bool IsCurrentSpot(CameraSpot spot)
        {
            ResolveStartSpot();
            return this.CurrentSpot == spot;
        }

        /** Finds the start spot by startSpotId if there is no current spot yet. */
        private void ResolveStartSpot()
        {
            if (CurrentSpot != null || string.IsNullOrWhiteSpace(startSpotId))
                return;

            if (registry == null)
            {
                Debug.LogError($"{nameof(SpotManager)} cannot resolve Start Spot Id because registry is missing.", this);
                return;
            }

            CurrentSpot = registry.GetSpot(startSpotId);
        }
    }
}
