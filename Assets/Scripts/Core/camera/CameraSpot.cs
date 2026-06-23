using Interaction.util.ColorReveal;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /*
     * Defines one camera spot in the scene.
     */
    public class CameraSpot : MonoBehaviour
    {
        [SerializeField] private string spotId;
        private IColorRevealable[] revealableChildren;
        [SerializeField] private CinemachineCamera spotCamera;
        [SerializeField] private RightClickCameraOrbit rightClickCameraOrbit;
        [SerializeField] private bool allowRightClickLook = true;
        private bool isLookControlActive;

        public string GetSpotId()
        {
            return spotId;
        }

        public CinemachineCamera getSpotCamera()
        {
            return this.spotCamera;
        }

        
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

        private void OnDisable()
        {
            CameraSpotRegistry registry = FindFirstObjectByType<CameraSpotRegistry>();
            if (registry == null)
                return;

            registry.UnregisterSpot(this);
        }

        // Reveals all objects in this spot.
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

        public void ResetLook()
        {
            if (rightClickCameraOrbit != null)
                rightClickCameraOrbit.ResetLook();
        }

        public void SetLookControlActive(bool isActive)
        {
            isLookControlActive = isActive;

            if (rightClickCameraOrbit == null)
                return;

            rightClickCameraOrbit.enabled = isLookControlActive && allowRightClickLook;
        }

        public void SetAllowRightClickLook(bool state)
        {
            allowRightClickLook = state;

            if (rightClickCameraOrbit == null)
                return;

            rightClickCameraOrbit.enabled = isLookControlActive && allowRightClickLook;
        }
    }
}
