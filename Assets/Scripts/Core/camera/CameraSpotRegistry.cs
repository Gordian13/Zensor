using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /**
     * Holds all the cameras and Spots, which self register after coming into existence.
     * Good Link: https://www.unitydesignpatterns.com/patterns/servicelocator
     */
    public class CameraSpotRegistry : MonoBehaviour
    {
        /** All registered spots, found by their spot id. */
        private readonly Dictionary<string, CameraSpot> spots = new();
        /** All registered route cameras, found by their camera id. */
        private readonly Dictionary<string, CinemachineCamera> cameras = new();

        /**
         * Adds a spot to the registry.
         * Logs an error if the spot is null, has no id or the id is already used by another spot.
         *
         * @param spot The spot to register.
         */
        public void RegisterSpot(CameraSpot spot)
        {
            if (spot == null)
            {
                Debug.LogError("Tried to register a null CameraSpot.", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(spot.GetSpotId()))
            {
                Debug.LogError($"CameraSpot '{spot.name}' has no spot id and cannot be registered.", spot);
                return;
            }

            if (spots.TryGetValue(spot.GetSpotId(), out CameraSpot existingSpot) && existingSpot != spot)
                Debug.LogError($"Duplicate CameraSpot id '{spot.GetSpotId()}' on '{existingSpot.name}' and '{spot.name}'.", this);

            spots[spot.GetSpotId()] = spot;
        }

        /**
         * Removes a spot from the registry, but only if it is the one registered under its id.
         *
         * @param spot The spot to remove.
         */
        public void UnregisterSpot(CameraSpot spot)
        {
            if (spot == null || string.IsNullOrWhiteSpace(spot.GetSpotId()))
                return;

            if (spots.TryGetValue(spot.GetSpotId(), out CameraSpot registeredSpot) && registeredSpot == spot)
                spots.Remove(spot.GetSpotId());
        }

        /**
         * Returns the spot with the given id.
         * If it is not registered yet, all CameraSpots in the scene get registered and it searches again.
         *
         * @param spotId The id of the spot.
         * @return The found spot, or null if there is none.
         */
        public CameraSpot GetSpot(string spotId)
        {
            if (string.IsNullOrWhiteSpace(spotId))
            {
                Debug.LogError("Cannot get CameraSpot because spot id is empty.", this);
                return null;
            }

            if (!spots.TryGetValue(spotId, out CameraSpot spot))
            {
                foreach (CameraSpot s in FindObjectsByType<CameraSpot>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    RegisterSpot(s);

                spots.TryGetValue(spotId, out spot);
            }

            if (spot == null)
                Debug.LogError($"No registered CameraSpot with id '{spotId}'.", this);

            return spot;
        }

        /**
         * Adds a route camera to the registry.
         * Logs an error if the camera is null, has no id, has no CinemachineCamera or the id is already used.
         *
         * @param routeCamera The route camera to register.
         */
        public void RegisterCamera(RouteCamera routeCamera)
        {
            if (routeCamera == null)
            {
                Debug.LogError("Tried to register a null RouteCamera.", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(routeCamera.GetCameraId()))
            {
                Debug.LogError($"RouteCamera '{routeCamera.name}' has no camera id and cannot be registered.", routeCamera);
                return;
            }

            if (routeCamera.Camera == null)
            {
                Debug.LogError($"RouteCamera '{routeCamera.GetCameraId()}' has no CinemachineCamera.", routeCamera);
                return;
            }

            if (cameras.TryGetValue(routeCamera.GetCameraId(), out CinemachineCamera existingCamera) &&
                existingCamera != routeCamera.Camera)
            {
                Debug.LogError($"Duplicate RouteCamera id '{routeCamera.GetCameraId()}'.", routeCamera);
            }

            cameras[routeCamera.GetCameraId()] = routeCamera.Camera;
        }

        /**
         * Removes a route camera from the registry, but only if it is the one registered under its id.
         *
         * @param routeCamera The route camera to remove.
         */
        public void UnregisterCamera(RouteCamera routeCamera)
        {
            if (routeCamera == null || string.IsNullOrWhiteSpace(routeCamera.GetCameraId()))
                return;

            if (cameras.TryGetValue(routeCamera.GetCameraId(), out CinemachineCamera registeredCamera) &&
                registeredCamera == routeCamera.Camera)
            {
                cameras.Remove(routeCamera.GetCameraId());
            }
        }

        /**
         * Returns the route camera with the given id.
         *
         * @param cameraId The id of the route camera.
         * @return The found CinemachineCamera, or null if there is none.
         */
        public CinemachineCamera GetCamera(string cameraId)
        {
            if (string.IsNullOrWhiteSpace(cameraId))
            {
                Debug.LogError("Cannot get RouteCamera because camera id is empty.", this);
                return null;
            }

            cameras.TryGetValue(cameraId, out CinemachineCamera camera);
            if (camera == null)
                Debug.LogError($"No registered RouteCamera with id '{cameraId}'.", this);

            return camera;
        }

        /**
         * Returns all cameras: first the route cameras, then the cameras of all spots.
         *
         * @return All registered CinemachineCameras.
         */
        public IEnumerable<CinemachineCamera> GetAllCameras()
        {
            foreach (CinemachineCamera routeCamera in cameras.Values)
                yield return routeCamera;

            foreach (CameraSpot spot in spots.Values)
            {
                if (spot != null && spot.getSpotCamera() != null)
                    yield return spot.getSpotCamera();
            }
        }
    }
}
