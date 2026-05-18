using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCInteractionMenu : MonoBehaviour
{
    public static NPCInteractionMenu Instance;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private Button buttonPrefab;

    private NPCController currentNPC;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    public void Open(NPCController npc)
    {
        if (npc == null || npc.Profile == null)
            return;

        currentNPC = npc;

        ClearButtons();

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
                Close();
            });
        }

        root.SetActive(true);
    }

    public void Close()
    {
        root.SetActive(false);
        ClearButtons();
    }

    private void ClearButtons()
    {
        for (int i = buttonParent.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonParent.GetChild(i).gameObject);
        }
    }
}