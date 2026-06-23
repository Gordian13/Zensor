using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /*
     * Plays route cameras between spots and lets Cinemachine handle each blend.
     */
    public class CameraTransitionManager : MonoBehaviour
    {
        [SerializeField] private SpotManager spotManager;
        [SerializeField] private CameraSpotRegistry registry;
        [SerializeField] private CinemachineBrain brain;
        [SerializeField] private float routeCameraTime = 0.25f;
        [SerializeField] private int inactivePriority = 0;
        [SerializeField] private int activePriority = 10;
        [SerializeField] private float finishDelay = 0.75f;

        private Coroutine currentTransition;
        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (spotManager == null)
                Debug.LogError($"{nameof(CameraTransitionManager)} has no SpotManager assigned.", this);

            if (registry == null)
                Debug.LogError($"{nameof(CameraTransitionManager)} has no CameraSpotRegistry assigned.", this);

            if (brain == null)
                brain = FindFirstObjectByType<CinemachineBrain>();
        }

        private void Start()
        {
            if (spotManager == null)
                return;

            CameraSpot currentSpot = spotManager.GetCurrentSpot();
            if (currentSpot != null)
                StartOnCamera(currentSpot.getSpotCamera());
        }

        public void PlayRoute(CameraRoute route, string destinationSpotId)
        {
            if (registry == null || string.IsNullOrWhiteSpace(destinationSpotId))
            {
                Debug.LogError("Cannot play transition because registry or destination spot id is missing.", this);
                return;
            }

            if (IsTransitioning)
                return;

            CameraSpot destinationSpot = registry.GetSpot(destinationSpotId);
            if (destinationSpot == null)
            {
                Debug.LogError($"Destination spot id '{destinationSpotId}' was not found.", this);
                return;
            }

            if (spotManager != null && spotManager.IsCurrentSpot(destinationSpot))
                return;

            currentTransition = StartCoroutine(PlayRouteRoutine(route, destinationSpot));
        }

        private IEnumerator PlayRouteRoutine(CameraRoute route, CameraSpot destinationSpot)
        {
            IsTransitioning = true;

            CameraSpot currentSpot = spotManager != null ? spotManager.GetCurrentSpot() : null;
            if (currentSpot != null)
                currentSpot.SetLookControlActive(false);

            CinemachineCamera destinationCamera = destinationSpot.getSpotCamera();

            if (route != null && route.wayCamerasIds != null)
            {
                foreach (string cameraId in route.wayCamerasIds)
                {
                    if (string.IsNullOrWhiteSpace(cameraId))
                        continue;

                    CinemachineCamera routeCamera = registry.GetCamera(cameraId);
                    if (routeCamera == null)
                    {
                        Debug.LogError($"Cannot play route because route camera id '{cameraId}' was not found.", this);
                        FinishTransition();
                        yield break;
                    }

                    if (routeCamera == destinationCamera)
                        continue;

                    ResetOrbitIfInactive(routeCamera);

                    if (!SetActiveCamera(routeCamera))
                    {
                        FinishTransition();
                        yield break;
                    }

                    yield return WaitForBlend(routeCameraTime);
                }
            }

            if (destinationCamera != null && destinationCamera.Priority != activePriority)
                destinationSpot.ResetLook();

            if (!SetActiveCamera(destinationCamera))
            {
                FinishTransition();
                yield break;
            }

            yield return WaitForBlend(finishDelay);

            if (spotManager != null)
                spotManager.SetCurrentSpot(destinationSpot);

            FinishTransition();
        }

        private bool SetActiveCamera(CinemachineCamera targetCamera)
        {
            if (targetCamera == null)
            {
                Debug.LogError("Cannot activate camera because targetCamera is missing.", this);
                return false;
            }

            foreach (CinemachineCamera cam in registry.GetAllCameras())
            {
                if (cam == null)
                {
                    Debug.LogError("Registry returned a null CinemachineCamera.", this);
                    return false;
                }

                cam.Priority = inactivePriority;
            }

            targetCamera.Priority = activePriority;
            return true;
        }

        private void StartOnCamera(CinemachineCamera targetCamera)
        {
            if (targetCamera == null)
                return;

            if (brain != null)
                brain.enabled = false;

            Camera outputCamera = brain != null ? brain.OutputCamera : Camera.main;
            if (outputCamera != null)
            {
                outputCamera.transform.SetPositionAndRotation(
                    targetCamera.transform.position,
                    targetCamera.transform.rotation
                );
            }

            SetActiveCamera(targetCamera);

            if (brain != null)
                StartCoroutine(EnableBrainAfterStartup());
        }

        private IEnumerator EnableBrainAfterStartup()
        {
            yield return null;
            brain.enabled = true;
        }

        private IEnumerator WaitForBlend(float minimumTime)
        {
            float elapsed = 0f;
            while (elapsed < minimumTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            while (brain != null && brain.enabled && brain.IsBlending)
            {
                yield return null;
            }
        }

        private void ResetOrbitIfInactive(CinemachineCamera camera)
        {
            if (camera == null || camera.Priority == activePriority)
                return;

            RightClickCameraOrbit orbit = camera.GetComponent<RightClickCameraOrbit>();
            if (orbit != null)
                orbit.ResetLook();
        }

        private void FinishTransition()
        {
            IsTransitioning = false;
            currentTransition = null;
        }
    }
}
