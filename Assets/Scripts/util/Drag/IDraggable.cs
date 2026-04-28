using UnityEngine;

public interface IDraggable
{
    void OnBeginDrag(Vector3 mouseWorldPos);
    void OnDrag(Vector3 mouseWorldPos);
    void OnEndDrag();
}
