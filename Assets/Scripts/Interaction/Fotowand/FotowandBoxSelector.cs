using Core.camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.FotowandSelect
{
    public class FotowandBoxSelector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private FotowandUI fotowandUI;

        [Header("Raycast")]
        [SerializeField] private LayerMask fotowandLayer = ~0;
        [SerializeField] private float rayDistance = 100f;

        [Header("Spot Guard")]
        [SerializeField] private SpotManager spotManager;
        [SerializeField] private CameraSpot owningSpot;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (spotManager == null)
                spotManager = FindFirstObjectByType<SpotManager>();

            if (owningSpot == null)
                owningSpot = GetComponentInParent<CameraSpot>();
        }

        private void Update()
        {
            if (!IsOwningSpotActive())
                return;

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;

            IFotowand hit = GetFotowandUnderCursor();
            if (hit != null)
                fotowandUI.Open(hit.GetData());
        }

        private IFotowand GetFotowandUnderCursor()
        {
            // Hier nochmal prüfen (nicht nur Awake), da Camera.main
            // Konnte in Awake aufgrund von Ladezeiten zwischen Szenen nicht aufgelöst werden.
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null || Mouse.current == null)
                return null;

            Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hitInfo, rayDistance, fotowandLayer))
                return null;

            return hitInfo.collider.GetComponentInParent<IFotowand>();
        }

        private bool IsOwningSpotActive()
        {
            if (spotManager == null)
                spotManager = FindFirstObjectByType<SpotManager>();

            if (spotManager == null || owningSpot == null)
                return true;

            return spotManager.IsCurrentSpot(owningSpot);
        }
    }
}