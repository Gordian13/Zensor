using UnityEngine;

namespace Core.camera
{
    /*
     * This Class is a placeHolder for the current selected Spot
     */
    public class SpotManager : MonoBehaviour
    {
        [SerializeField] private CameraSpot _startSpot;
        [SerializeField] private string startSpotId;
        [SerializeField] private CameraSpotRegistry registry;

        private CameraSpot CurrentSpot;

        private void Awake()
        {
            CurrentSpot = _startSpot;

            if (registry == null)
                Debug.LogError($"{nameof(SpotManager)} has no CameraSpotRegistry assigned.", this);
        }

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

        public CameraSpot GetCurrentSpot()
        {
            ResolveStartSpot();
            return CurrentSpot;
        }

        public string GetCurrentSpotId()
        {
            if (CurrentSpot == null)
            {
                Debug.LogError($"{nameof(SpotManager)} has no current spot.", this);
                return string.Empty;
            }

            return CurrentSpot.GetSpotId();
        }

        public string GetStartSpotId()
        {
            if (!string.IsNullOrWhiteSpace(startSpotId))
                return startSpotId;

            if (_startSpot != null)
                return _startSpot.GetSpotId();

            Debug.LogError($"{nameof(SpotManager)} has no start spot assigned.", this);
            return string.Empty;
        }

        public bool IsCurrentSpot(CameraSpot spot)
        {
            ResolveStartSpot();
            return this.CurrentSpot == spot;
        }

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
