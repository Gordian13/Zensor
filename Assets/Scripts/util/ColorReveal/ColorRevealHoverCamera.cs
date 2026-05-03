using System;
using DefaultNamespace.GrayScale;
using UnityEngine;
using UnityEngine.InputSystem;

namespace util.ColorReveal
{
    public class ColorRevealHoverCamera : MonoBehaviour
    {
        private Camera _camera;
        private IColorRevealable _currentHovered;
        private IColorRevealable _lastHovered;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            IColorRevealable currentHovered = GetHoveredIColorRevealable();

            Debug.Log($"currentHovered: {(currentHovered == null ? "null" : currentHovered.ToString())}");

            if (Equals(currentHovered, _lastHovered))
                return;

            if (_lastHovered != null)
                _lastHovered.ToggleColor(); 

            if (currentHovered != null)
                currentHovered.ToggleColor(); 

            _lastHovered = currentHovered;
        }

        private IColorRevealable GetHoveredIColorRevealable()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
                return null;

            Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << 3))
                return null;

            return hit.collider.GetComponentInParent<IColorRevealable>();
        }
    }
}