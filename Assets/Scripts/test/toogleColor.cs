using DefaultNamespace.GrayScale;
using UnityEngine;
using UnityEngine.InputSystem;
using util.GrayScale;

public class toogleColor : MonoBehaviour
{
    public ColorRevealToggle itemToToggle;

    void Update()
    {
        var mouse = Mouse.current;
        if (!mouse.leftButton.wasPressedThisFrame) return;

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Raycast hit: {hit.transform.name}");
            if (hit.transform == gameObject.transform)
                itemToToggle.ToggleColor();
        }
    }
}
