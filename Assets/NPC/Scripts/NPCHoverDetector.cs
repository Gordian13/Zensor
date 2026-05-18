using UnityEngine;
using UnityEngine.InputSystem;

public class NPCHoverDetector : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private LayerMask npcLayerMask = ~0;
    [SerializeField] private float maxDistance = 100f;

    private NPCHoverHighlight currentHovered;

    private void Awake()
    {
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || raycastCamera == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = raycastCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, npcLayerMask))
        {
            NPCHoverHighlight hover = hit.collider.GetComponentInParent<NPCHoverHighlight>();
            
            if (hover != currentHovered)
            {
                ClearCurrentHover();
                currentHovered = hover;

                if (currentHovered != null)
                    currentHovered.SetHighlight(true);
            }
        }
        else
        {
            ClearCurrentHover();
        }
    }

    private void ClearCurrentHover()
    {
        if (currentHovered != null)
        {
            currentHovered.SetHighlight(false);
            currentHovered = null;
        }
    }
}