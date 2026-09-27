using UnityEngine;

namespace Core.camera
{
    /**
     * How can we get to this spot?
     *
     * Clickable object that leads to a target spot and knows the routes to get there.
     */
    public class SpotNavigationTrigger : MonoBehaviour
    {
        /** The spot the camera moves to when this trigger is clicked. */
        [SerializeField] private CameraSpot targetSpot;
        /** Routes to the target spot, one for each start spot. */
        [SerializeField] private CameraRoute[] routes;

        /** @return The id of the target spot, or an empty string if no target spot is set. */
        public string GetTargetSpotId()
        {
            if (targetSpot == null)
            {
                Debug.LogError($"{nameof(SpotNavigationTrigger)} on {name} has no target spot assigned.", this);
                return string.Empty;
            }

            return targetSpot != null ? targetSpot.GetSpotId() : string.Empty;
        }

        /**
         * Returns the route matching the current spot, or null when none is
         * defined so the transition falls back to a direct blend.
         *
         * @param currentSpotId The id of the spot the camera is at right now.
         * @return The matching route, or null.
         */
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
