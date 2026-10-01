using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /**
     * Plays route cameras between spots and lets Cinemachine handle each blend.
     */
    public class CameraTransitionManager : MonoBehaviour
    {
        /** The single instance of the manager (Singleton Pattern). */
        public static CameraTransitionManager Instance { get; private set; }

        /** Knows the current spot. */
        [SerializeField] private SpotManager spotManager;
        /** Used to find the spots and route cameras by id. */
        [SerializeField] private CameraSpotRegistry registry;
        /** The CinemachineBrain on the main camera, searched in Awake if not set. */
        [SerializeField] private CinemachineBrain brain;
        /** Minimum time each route camera stays active (in seconds). */
        [SerializeField] private float routeCameraTime = 0.25f;
        /** Priority of all cameras that are not active. */
        [SerializeField] private int inactivePriority = 0;
        /** Priority of the active camera, Cinemachine blends to the camera with the highest priority. */
        [SerializeField] private int activePriority = 10;
        /** Minimum time before the transition to the destination spot counts as finished (in seconds). */
        [SerializeField] private float finishDelay = 0.75f;

        /** The transition that is running right now, or null. */
        private Coroutine currentTransition;
        /** True while the camera moves between spots. */
        public bool IsTransitioning { get; private set; }

        /** Sets up the Singleton, checks the references and searches the CinemachineBrain. */
        private void Awake()
        {
            Instance = this;

            if (spotManager == null)
                Debug.LogError($"{nameof(CameraTransitionManager)} has no SpotManager assigned.", this);

            if (registry == null)
                Debug.LogError($"{nameof(CameraTransitionManager)} has no CameraSpotRegistry assigned.", this);

            if (brain == null)
                brain = FindFirstObjectByType<CinemachineBrain>();
        }

        /** Puts the camera directly on the start spot without a blend. */
        private void Start()
        {
            if (spotManager == null)
                return;

            CameraSpot currentSpot = spotManager.GetCurrentSpot();
            if (currentSpot != null)
                StartOnCamera(currentSpot.getSpotCamera());
        }

        /**
         * Moves the camera to the spot with the given id.
         * Does nothing if a transition is already running or the spot is already current.
         *
         * @param route The route to take, or null for a direct blend.
         * @param destinationSpotId The id of the spot to move to.
         */
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

        /**
         * Moves the camera to the given spot.
         * Does nothing if a transition is already running or the spot is already current.
         *
         * @param route The route to take, or null for a direct blend.
         * @param destinationSpot The spot to move to.
         */
        public void PlayRoute(CameraRoute route, CameraSpot destinationSpot)
        {
            if (destinationSpot == null)
            {
                Debug.LogError("Cannot play transition because destination spot is null.", this);
                return;
            }

            if (IsTransitioning)
                return;

            if (spotManager != null && spotManager.IsCurrentSpot(destinationSpot))
                return;
            currentTransition = StartCoroutine(PlayRouteRoutine(route, destinationSpot));
        }

        /**
         * Plays the transition: activates every route camera one after the other and then the destination camera.
         * Blocks all interactions while it runs and sets the new current spot at the end.
         *
         * @param route The route to take, or null for a direct blend.
         * @param destinationSpot The spot to move to.
         */
        private IEnumerator PlayRouteRoutine(CameraRoute route, CameraSpot destinationSpot)
        {
            IsTransitioning = true;

            CameraSpot currentSpot = spotManager != null ? spotManager.GetCurrentSpot() : null;
            if (currentSpot != null)
                currentSpot.SetLookControlActive(false);

            CinemachineCamera destinationCamera = destinationSpot.getSpotCamera();

            GlobalInteractionState.Instance.BlockInteractions();

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

        /**
         * Gives the target camera the active priority and all other cameras the inactive priority.
         *
         * @param targetCamera The camera to activate.
         * @return False if the target camera is missing or the registry returns a null camera.
         */
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

        /**
         * Puts the output camera directly on the target camera without a blend.
         * The brain is turned off for one frame so it does not blend from the old position.
         *
         * @param targetCamera The camera to start on.
         */
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

        /** Turns the CinemachineBrain back on after one frame. */
        private IEnumerator EnableBrainAfterStartup()
        {
            yield return null;
            brain.enabled = true;
        }

        /**
         * Waits at least minimumTime and then until Cinemachine has finished blending.
         *
         * @param minimumTime Minimum time to wait (in seconds).
         */
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

        /**
         * Turns the camera back to its start rotation if it is not active right now.
         *
         * @param camera The camera to reset.
         */
        private void ResetOrbitIfInactive(CinemachineCamera camera)
        {
            if (camera == null || camera.Priority == activePriority)
                return;

            RightClickCameraOrbit orbit = camera.GetComponent<RightClickCameraOrbit>();
            if (orbit != null)
                orbit.ResetLook();
        }

        /** Ends the transition and unblocks the interactions. */
        private void FinishTransition()
        {
            IsTransitioning = false;
            currentTransition = null;
            GlobalInteractionState.Instance.UnblockInteractions();
        }
    }
}
