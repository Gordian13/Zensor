using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /*
     * script to register the cams on the way of the transition
     */
    [RequireComponent(typeof(CinemachineCamera))]
    public class RouteCamera : MonoBehaviour
    {
        [SerializeField] private string cameraId;

        public CinemachineCamera Camera { get; private set; }

        public string GetCameraId()
        {
            return cameraId;
        }

        private void Awake()
        {
            Camera = GetComponent<CinemachineCamera>();

            if (string.IsNullOrWhiteSpace(cameraId))
                Debug.LogError($"{nameof(RouteCamera)} on {name} has no camera id.", this);

            if (Camera == null)
                Debug.LogError($"{nameof(RouteCamera)} '{cameraId}' has no CinemachineCamera component.", this);
        }

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

        private void OnDisable()
        {
            CameraSpotRegistry registry = FindFirstObjectByType<CameraSpotRegistry>();
            if (registry != null)
                registry.UnregisterCamera(this);
        }
    }
}
