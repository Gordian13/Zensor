using UnityEngine;

namespace Core.camera
{
    public class SpotNavigationTrigger : MonoBehaviour
    {
        [SerializeField] private CameraSpot targetSpot;
        [SerializeField] private CameraRoute[] routes;

        public string GetTargetSpotId()
        {
            if (targetSpot == null)
            {
                Debug.LogError($"{nameof(SpotNavigationTrigger)} on {name} has no target spot assigned.", this);
                return string.Empty;
            }

            return targetSpot != null ? targetSpot.GetSpotId() : string.Empty;
        }

        public CameraRoute GetRouteFrom(string currentSpotId)
        {
            if (string.IsNullOrWhiteSpace(currentSpotId))
            {
                Debug.LogError($"{nameof(SpotNavigationTrigger)} on {name} cannot find a route because currentSpotId is empty.", this);
                return null;
            }

            if (routes == null || routes.Length == 0)
            {
                Debug.LogError($"{nameof(SpotNavigationTrigger)} on {name} has no routes assigned.", this);
                return null;
            }

            if (routes != null)
            {
                foreach (CameraRoute route in routes)
                {
                    if (route == null)
                    {
                        Debug.LogError($"{nameof(SpotNavigationTrigger)} on {name} has a null route entry.", this);
                        return null;
                    }

                    if (route.fromSpotId == currentSpotId)
                        return route;
                }
            }

            Debug.LogError($"No route from spot id '{currentSpotId}' on {name}.", this);
            return null;
        }
    }
}
