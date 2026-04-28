using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    private IDraggable _currentDraggable;
    private float _zCoord;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                IDraggable draggable = hit.transform.GetComponent<IDraggable>();
                if (draggable != null)
                {
                    _currentDraggable = draggable;
                    _zCoord = Camera.main.WorldToScreenPoint(hit.transform.position).z;
                    _currentDraggable.OnBeginDrag(GetMouseWorldPos());
                }
            }
        }

        if (_currentDraggable != null)
            _currentDraggable.OnDrag(GetMouseWorldPos());

        if (mouse.leftButton.wasReleasedThisFrame && _currentDraggable != null)
        {
            _currentDraggable.OnEndDrag();
            _currentDraggable = null;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        mousePoint.z = _zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
