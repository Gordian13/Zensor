using UnityEngine;

namespace Core.camera
{
    /*
     * how can we get to this spot?
     */
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

        // Returns the route matching the current spot, or null when none is
        // defined so the transition falls back to a direct blend.
        public CameraRoute GetRouteFrom(string currentSpotId)
        {
            if (string.IsNullOrWhiteSpace(currentSpotId))
            {
                Debug.LogError($"{nameof(SpotNavigationTrigger)} on {name} cannot find a route because currentSpotId is empty.", this);
                return null;
            }

            if (routes == null || routes.Length == 0)
                return null;

            foreach (CameraRoute route in routes)
            {
                if (route == null)
                {
                    Debug.LogError($"{nameof(SpotNavigationTrigger)} on {name} has a null route entry.", this);
                    continue;
                }

                if (route.fromSpotId == currentSpotId)
                    return route;
            }

            return null;
        }
    }
}
