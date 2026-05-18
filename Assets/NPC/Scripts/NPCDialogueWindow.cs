using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class NPCDialogueWindow : MonoBehaviour
{
    public static NPCDialogueWindow Instance;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private float dialogueDisplayTime = 5f;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    public void Show(string text)
    {
        if (root == null || dialogueText == null)
        {
            Debug.LogError("NPCDialogueWindow is missing UI references.");
            return;
        }

        dialogueText.text = text;
        root.SetActive(true);
        Invoke(nameof(Hide), dialogueDisplayTime);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}