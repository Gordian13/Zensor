using UnityEngine;
using UnityEngine.InputSystem;

namespace Interaction.util.ColorReveal
{
    /**
     * Camera script that detects IColorRevealable objects under the mouse cursor
     * and sets their color state when the hovered object changes.
     */
    public class ColorRevealHoverCamera : MonoBehaviour
    {
        [SerializeField] private float maxDistanceMeter = 100f;
        [SerializeField] private LayerMask hoverLayer = ~0;

        private Camera _camera;
        private IColorRevealable _lastHovered;

        private void Awake()
        {
            _camera = Camera.main;

            if (_camera == null)
                Debug.LogError($"{nameof(ColorRevealHoverCamera)}: no Camera.main found in the scene.", this);
        }

        /**
         * Checks every frame whether the currently hovered IColorRevealable has changed.
         * If it has, the previously hovered object is un-revealed and the new one is revealed.
         */
        private void Update()
        {
            IColorRevealable currentHovered = GetHoveredIColorRevealable();

            if (Equals(currentHovered, _lastHovered))
                return;

            _lastHovered?.SetColor(false);
            currentHovered?.SetColor(true);

            _lastHovered = currentHovered;
        }

        /**
         * Returns the IColorRevealable currently under the mouse cursor, or null if none is hit.
         * Uses a raycast from the camera through the mouse position on the configured hover layer.
         */
        private IColorRevealable GetHoveredIColorRevealable()
        {
            if (_camera == null) return null;

            Mouse mouse = Mouse.current;
            if (mouse == null) return null;

            Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistanceMeter, hoverLayer))
                return null;

            return hit.collider.GetComponentInParent<IColorRevealable>();
        }
    }
}
