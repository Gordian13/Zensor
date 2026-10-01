using UnityEngine;

public class GlobalInteractionState : MonoBehaviour
{
    public static GlobalInteractionState Instance { get; private set; }

    public bool IsInteractionBlocked { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void BlockInteractions()
    {
        IsInteractionBlocked = true;
        Debug.Log("GLOBAL BLOCK ON");
    }

    public void UnblockInteractions()
    {
        IsInteractionBlocked = false;
        Debug.Log("GLOBAL BLOCK OFF");
    }
}