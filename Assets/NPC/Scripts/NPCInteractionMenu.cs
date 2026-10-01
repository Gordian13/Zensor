using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds a menu from the interactions stored in an NPC profile.
/// The menu closes the related dialogue and interaction together.
/// </summary>
public class NPCInteractionMenu : MonoBehaviour
{
    /// <summary>The active NPC interaction menu in the scene.</summary>
    public static NPCInteractionMenu Instance;

    /// <summary>Top-level menu object that is enabled only while the interaction menu is open.</summary>
    [Header("UI")]
    [Tooltip("Top-level interaction menu object that is shown and hidden by this component.")]
    [SerializeField] private GameObject root;

    /// <summary>Parent transform that receives all generated interaction buttons.</summary>
    [Tooltip("Container under which interaction buttons are generated. Its children are removed whenever the menu is rebuilt.")]
    [SerializeField] private Transform buttonParent;

    /// <summary>
    /// Button prefab cloned once for every non-null <see cref="NPCInteraction"/> in the NPC profile.
    /// It should contain a TMP_Text child because the interaction name is written there.
    /// </summary>
    [Tooltip("Button prefab used for each interaction. It should contain a TextMeshPro text child for the interaction name.")]
    [SerializeField] private Button buttonPrefab;

    /// <summary>Optional message shown when the selected NPC profile contains no interactions.</summary>
    [Tooltip("Optional message shown when the NPC profile has no interaction entries.")]
    [SerializeField] private TMP_Text emptyText;

    /// <summary>NPC whose profile currently supplies the menu entries.</summary>
    private NPCController currentNPC;

    /// <summary>Pending delayed-close operation, if one was requested.</summary>
    private Coroutine closeRoutine;

    /// <summary>Registers the menu and hides its UI at startup.</summary>
    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (emptyText != null)
            emptyText.gameObject.SetActive(false);
    }

    /// <summary>Opens the menu and creates a button for each available interaction.</summary>
    /// <param name="npc">The NPC whose interactions should be shown.</param>
    public void Open(NPCController npc)
    {
        if (npc == null || npc.Profile == null)
            return;

        currentNPC = npc;
        ClearButtons();
        bool hasInteractions =
            npc.Profile.interactions != null &&
            npc.Profile.interactions.Count > 0;

        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasInteractions);

        if (hasInteractions)
        {
            foreach (NPCInteraction interaction in npc.Profile.interactions)
            {
                if (interaction == null)
                    continue;

                Button button = Instantiate(buttonPrefab, buttonParent);
                TMP_Text text = button.GetComponentInChildren<TMP_Text>();

                if (text != null)
                    text.text = interaction.interactionName;

                button.onClick.AddListener(() =>
                {
                    interaction.Execute(currentNPC);
                });
            }
        }

        root.SetActive(true);
    }

    /// <summary>Closes the menu and ends the current NPC interaction.</summary>
    public void Close()
    {
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        if (root != null)
            root.SetActive(false);

        ClearButtons();

        if (emptyText != null)
            emptyText.gameObject.SetActive(false);

        if (NPCDialogueWindow.Instance != null)
            NPCDialogueWindow.Instance.Hide();

        if (currentNPC != null)
            currentNPC.EndInteraction();

        currentNPC = null;
    }

    /// <summary>Schedules the menu to close after a delay.</summary>
    /// <param name="seconds">Delay in seconds.</param>
    public void CloseAfterDelay(float seconds)
    {
        if (closeRoutine != null)
            StopCoroutine(closeRoutine);

        closeRoutine = StartCoroutine(CloseAfterDelayRoutine(seconds));
    }

    /// <summary>Waits for the requested delay and then closes the menu.</summary>
    /// <param name="seconds">Delay in seconds.</param>
    /// <returns>An enumerator used by Unity as a coroutine.</returns>
    private System.Collections.IEnumerator CloseAfterDelayRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Close();
    }

    /// <summary>Removes all interaction buttons from the menu.</summary>
    private void ClearButtons()
    {
        if (buttonParent == null)
            return;

        for (int i = buttonParent.childCount - 1; i >= 0; i--)
            Destroy(buttonParent.GetChild(i).gameObject);
    }
}
