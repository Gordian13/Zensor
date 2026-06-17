using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class NPCInteractionMenu : MonoBehaviour
{
    public static NPCInteractionMenu Instance;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private TMP_Text emptyText;

    private NPCController currentNPC;
    private Coroutine closeRoutine;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (emptyText != null)
            emptyText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Deprecated, was used to allow closing the menu with the Escape key.
        /*if (root != null && root.activeSelf && Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }*/
    }

    public void Open(NPCController npc)
    {
        if (npc == null || npc.Profile == null)
            return;

        currentNPC = npc;
        ClearButtons();

        bool hasInteractions = npc.Profile.interactions != null && npc.Profile.interactions.Count > 0;

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

    public void CloseAfterDelay(float seconds)
    {
        if (closeRoutine != null)
            StopCoroutine(closeRoutine);

        closeRoutine = StartCoroutine(CloseAfterDelayRoutine(seconds));
    }

    private System.Collections.IEnumerator CloseAfterDelayRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Close();
    }

    private void ClearButtons()
    {
        if (buttonParent == null)
            return;

        for (int i = buttonParent.childCount - 1; i >= 0; i--)
            Destroy(buttonParent.GetChild(i).gameObject);
    }
}