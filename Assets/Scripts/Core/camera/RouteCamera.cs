using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /**
     * Script to register the cams on the way of the transition.
     */
    [RequireComponent(typeof(CinemachineCamera))]
    public class RouteCamera : MonoBehaviour
    {
        /** Unique id of this camera, used in CameraRoute.wayCamerasIds. */
        [SerializeField] private string cameraId;

        /** The CinemachineCamera on this GameObject. */
        public CinemachineCamera Camera { get; private set; }

        /** @return The id of this camera. */
        public string GetCameraId()
        {
            return cameraId;
        }

        /** Gets the CinemachineCamera and logs an error if the id or the camera is missing. */
        private void Awake()
        {
            Camera = GetComponent<CinemachineCamera>();

            if (string.IsNullOrWhiteSpace(cameraId))
                Debug.LogError($"{nameof(RouteCamera)} on {name} has no camera id.", this);

            if (Camera == null)
                Debug.LogError($"{nameof(RouteCamera)} '{cameraId}' has no CinemachineCamera component.", this);
        }

        /** Registers this camera in the CameraSpotRegistry. */
        private void OnEnable()
        {
            CameraSpotRegistry registry = FindFirstObjectByType<CameraSpotRegistry>();
            if (registry == null)
            {
                Debug.LogError($"{nameof(RouteCamera)} '{cameraId}' could not find a {nameof(CameraSpotRegistry)}.", this);
                return;
            }

            registry.RegisterCamera(this);
        }

        /** Removes this camera from the CameraSpotRegistry. */
        private void OnDisable()
        {
            CameraSpotRegistry registry = FindFirstObjectByType<CameraSpotRegistry>();
            if (registry != null)
                registry.UnregisterCamera(this);
        }
    }
}
