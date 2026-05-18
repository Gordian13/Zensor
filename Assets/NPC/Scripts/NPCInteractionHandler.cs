using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractionHandler : MonoBehaviour
{
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private LayerMask npcLayerMask = ~0;
    [SerializeField] private float maxDistance = 100f;

    private void Awake()
    {
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || raycastCamera == null)
            return;

        if (!Mouse.current.rightButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = raycastCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, npcLayerMask))
            return;

        NPCController npc = hit.collider.GetComponentInParent<NPCController>();

        if (npc == null)
            return;

        NPCInteractionMenu.Instance.Open(npc);
    }
}