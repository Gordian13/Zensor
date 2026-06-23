using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.camera
{
    /*
     * Holds all the cameras and Spots, which self register after coming into existens
     * Good Link: https://www.unitydesignpatterns.com/patterns/servicelocator
     */
    public class CameraSpotRegistry : MonoBehaviour
    {
        private readonly Dictionary<string, CameraSpot> spots = new();
        private readonly Dictionary<string, CinemachineCamera> cameras = new();

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

        public void UnregisterSpot(CameraSpot spot)
        {
            if (spot == null || string.IsNullOrWhiteSpace(spot.GetSpotId()))
                return;

            if (spots.TryGetValue(spot.GetSpotId(), out CameraSpot registeredSpot) && registeredSpot == spot)
                spots.Remove(spot.GetSpotId());
        }

        public CameraSpot GetSpot(string spotId)
        {
            if (string.IsNullOrWhiteSpace(spotId))
            {
                Debug.LogError("Cannot get CameraSpot because spot id is empty.", this);
                return null;
            }

            spots.TryGetValue(spotId, out CameraSpot spot);
            if (spot == null)
                Debug.LogError($"No registered CameraSpot with id '{spotId}'.", this);

            return spot;
        }

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
