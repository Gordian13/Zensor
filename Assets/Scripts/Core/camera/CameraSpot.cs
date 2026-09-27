using Interaction.util.ColorReveal;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /**
     * Defines one camera spot in the scene.
     *
     * A spot has its own camera and registers itself in the CameraSpotRegistry.
     */
    public class CameraSpot : MonoBehaviour
    {
        /** Unique id of this spot, used by the registry and the navigation triggers. */
        [SerializeField] private string spotId;
        /** All objects in this spot that turn colored when the spot is revealed. */
        private IColorRevealable[] revealableChildren;
        /** The camera that is active while the player is at this spot. */
        [SerializeField] private CinemachineCamera spotCamera;
        /** Lets the player look around with right click, taken from the spotCamera if not set. */
        [SerializeField] private RightClickCameraOrbit rightClickCameraOrbit;
        /** If false, right click look is turned off even while this spot is active (e.g. during vinyl inspection). */
        [SerializeField] private bool allowRightClickLook = true;
        /** True while this is the current spot. */
        private bool isLookControlActive;

        /** @return The id of this spot. */
        public string GetSpotId()
        {
            return spotId;
        }

        /** @return The camera of this spot. */
        public CinemachineCamera getSpotCamera()
        {
            return this.spotCamera;
        }

        
        /** Checks the references, collects all revealable children and turns the look control off. */
        public void Awake()
        {
            if (string.IsNullOrWhiteSpace(spotId))
                Debug.LogError($"{nameof(CameraSpot)} on {name} has no spot id.", this);

            if (spotCamera == null)
                Debug.LogError($"{nameof(CameraSpot)} '{spotId}' has no spot camera assigned.", this);

            if (rightClickCameraOrbit == null && spotCamera != null)
                rightClickCameraOrbit = spotCamera.GetComponent<RightClickCameraOrbit>();

            revealableChildren = GetComponentsInChildren<IColorRevealable>(true);
            if (revealableChildren.Length == 0)
                Debug.LogWarning($"{nameof(CameraSpot)} '{spotId}' found no revealable children.", this);

            SetLookControlActive(false);
        }

        /** Registers this spot in the CameraSpotRegistry. */
        private void OnEnable()
        {
            CameraSpotRegistry registry = FindFirstObjectByType<CameraSpotRegistry>();
            if (registry == null)
            {
                Debug.LogError($"{nameof(CameraSpot)} '{spotId}' could not find a {nameof(CameraSpotRegistry)}.", this);
                return;
            }

            registry.RegisterSpot(this);
        }

        /** Removes this spot from the CameraSpotRegistry. */
        private void OnDisable()
        {
            CameraSpotRegistry registry = FindFirstObjectByType<CameraSpotRegistry>();
            if (registry == null)
                return;

            registry.UnregisterSpot(this);
        }

        /**
         * Reveals all objects in this spot.
         *
         * @param isReveal True to show them in color, false for gray.
         */
        public void SetSpotReveal(bool isReveal)
        {
            if (revealableChildren == null)
            {
                Debug.LogError($"{nameof(CameraSpot)} '{spotId}' cannot reveal because revealableChildren was not initialized.", this);
                return;
            }

            foreach (var reveal in revealableChildren)
            {
                reveal.SetColorReveal(isReveal);
            }
        }

        /** Turns the camera back to its start rotation. */
        public void ResetLook()
        {
            if (rightClickCameraOrbit != null)
                rightClickCameraOrbit.ResetLook();
        }

        /**
         * Turns the right click look on or off, called when the spot becomes current or not.
         * The look only works if allowRightClickLook is also true.
         *
         * @param isActive True if this is the current spot.
         */
        public void SetLookControlActive(bool isActive)
        {
            isLookControlActive = isActive;

            if (rightClickCameraOrbit == null)
                return;

            rightClickCameraOrbit.enabled = isLookControlActive && allowRightClickLook;
        }

        /**
         * Allows or blocks the right click look, e.g. while a vinyl is selected.
         *
         * @param state True to allow looking around.
         */
        public void SetAllowRightClickLook(bool state)
        {
            allowRightClickLook = state;

            if (rightClickCameraOrbit == null)
                return;

            rightClickCameraOrbit.enabled = isLookControlActive && allowRightClickLook;
        }
    }
}
