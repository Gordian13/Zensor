using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Controls the right-click NPC interaction menu.
// It dynamically creates one UI button for every interaction assigned to the NPC profile.
public class NPCInteractionMenu : MonoBehaviour
{
    // Singleton-style access so NPCInteractionHandler can call:
    // NPCInteractionMenu.Instance.Open(npc);
    public static NPCInteractionMenu Instance;

    [Header("UI")]
    // Root panel of the interaction menu.
    // This is activated when the menu opens and deactivated when it closes.
    [SerializeField] private GameObject root;

    // Parent object under which generated interaction buttons are placed.
    [SerializeField] private Transform buttonParent;

    // Button prefab used to create one button per NPC interaction.
    [SerializeField] private Button buttonPrefab;

    // The NPC currently being interacted with.
    private NPCController currentNPC;

    private void Awake()
    {
        // Register this menu as the active global instance.
        Instance = this;

        // Start with the menu hidden.
        if (root != null)
            root.SetActive(false);
    }

    // Opens the interaction menu for a specific NPC.
    public void Open(NPCController npc)
    {
        // Without an NPC or profile, there is no interaction data to show.
        if (npc == null || npc.Profile == null)
            return;

        currentNPC = npc;

        // Remove old buttons from the previous NPC/menu opening.
        ClearButtons();

        // Create one button for every interaction assigned in the NPCProfile.
        foreach (NPCInteraction interaction in npc.Profile.interactions)
        {
            // Ignore missing interaction references.
            if (interaction == null)
                continue;

            // Create a new button from the prefab under buttonParent.
            Button button = Instantiate(buttonPrefab, buttonParent);

            // Find the TMP text inside the button and set its label.
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();

            if (text != null)
                text.text = interaction.interactionName;

            // When clicked, execute the interaction on the currently selected NPC,
            // then close the menu.
            button.onClick.AddListener(() =>
            {
                interaction.Execute(currentNPC);
                Close();
            });
        }

        // Show the menu after buttons have been generated.
        root.SetActive(true);
    }

    // Closes the menu and clears generated buttons.
    public void Close()
    {
        root.SetActive(false);
        ClearButtons();
    }

    // Deletes all dynamically created interaction buttons.
    private void ClearButtons()
    {
        // Iterate backwards because child count changes as objects are destroyed.
        for (int i = buttonParent.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonParent.GetChild(i).gameObject);
        }
    }
}
