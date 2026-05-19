using UnityEngine;
using TMPro;
using Unity.VisualScripting; // Currently not used in this script. Can be removed if no Visual Scripting features are added later.

// Controls the UI window that displays NPC dialogue text.
// This is intentionally kept simple: other NPC systems call Show(text), and this class handles the UI display.
public class NPCDialogueWindow : MonoBehaviour
{
    // Singleton-style reference so other scripts can call:
    // NPCDialogueWindow.Instance.Show("some text");
    //
    // This is simple and fine for the prototype.
    // For a larger project, a more robust UI manager would be cleaner.
    public static NPCDialogueWindow Instance;

    [Header("UI")]
    // The root GameObject of the dialogue window.
    // Usually this is the panel object itself.
    // It gets activated when dialogue is shown and deactivated when hidden.
    [SerializeField] private GameObject root;

    // TextMeshPro text component where the NPC dialogue is written.
    [SerializeField] private TMP_Text dialogueText;

    // How long the dialogue window stays visible before hiding automatically.
    [SerializeField] private float dialogueDisplayTime = 5f;

    private void Awake()
    {
        // Register this dialogue window as the globally accessible instance.
        Instance = this;

        // Start hidden so the dialogue box is not visible immediately when the scene starts.
        if (root != null)
            root.SetActive(false);
    }

    // Shows the dialogue window and writes the given text into it.
    public void Show(string text)
    {
        // Defensive check: without root or text reference, the UI cannot work.
        if (root == null || dialogueText == null)
        {
            Debug.LogError("NPCDialogueWindow is missing UI references.");
            return;
        }

        // Set the visible dialogue text.
        dialogueText.text = text;

        // Make the dialogue window visible.
        root.SetActive(true);

        // Automatically hide the dialogue after a delay.
        CancelInvoke(nameof(Hide)); // Cancel any previous hide calls to prevent stacking.
        Invoke(nameof(Hide), dialogueDisplayTime);
    }

    // Hides the dialogue window.
    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}
