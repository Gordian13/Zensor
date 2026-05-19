using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Displays the metadata of the currently selected record in the UI panel.
// This script only updates UI elements; it does not decide which record was selected.
public class RecordInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text authorText;
    [SerializeField] private TMP_Text yearText;
    [SerializeField] private TMP_Text playableText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image coverImage;

    [Header("Fallback Text")]
    [SerializeField] private string unknownTitle = "Unknown Title";
    [SerializeField] private string unknownAuthor = "Unknown Artist";
    [SerializeField] private string unknownYear = "Unknown Year";
    [SerializeField] private string unknownPlayable = "Unknown Status";
    [SerializeField] private string unknownDescription = "No description available.";

    // Used by the editor setup tool to connect the generated UI elements automatically.
    public void Configure(
        GameObject panel,
        TMP_Text title,
        TMP_Text author,
        TMP_Text year,
        TMP_Text playable,
        TMP_Text description,
        Image cover)
    {
        panelRoot = panel;
        titleText = title;
        authorText = author;
        yearText = year;
        playableText = playable;
        descriptionText = description;
        coverImage = cover;
    }

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        Hide();
    }

    // Called when a record is selected. It reads the record metadata and fills the UI panel.
    public void ShowData(RecordData data)
    {
        // If no record data is available, show neutral placeholder text instead.
        if (data == null)
        {
            Clear();
            SetPanelVisible(true);
            return;
        }

        // Each UI field is checked before use, so the panel still works if one reference is missing.
        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(data.title) ? unknownTitle : data.title;
        }

        if (authorText != null)
        {
            authorText.text = string.IsNullOrWhiteSpace(data.author) ? unknownAuthor : data.author;
        }

        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrWhiteSpace(data.description) ? unknownDescription : data.description;
        }

        if (yearText != null)
        {
            yearText.text = string.IsNullOrWhiteSpace(data.year) ? unknownYear : data.year;
        }

        if (playableText != null)
        {
            playableText.text = data.format == RecordFormat.Vinyl ? "Vinyl" : "Tape";
        }

        if (coverImage != null)
        {
            coverImage.sprite = data.sprite;
            coverImage.enabled = data.sprite != null;
        }

        SetPanelVisible(true);
    }

    // Hides the metadata panel when no record is selected or the user clicks away.
    public void Hide()
    {
        SetPanelVisible(false);
    }

    // Resets the UI to safe placeholder values so old record data is not left on screen.
    public void Clear()
    {
        if (titleText != null)
        {
            titleText.text = unknownTitle;
        }

        if (authorText != null)
        {
            authorText.text = unknownAuthor;
        }

        if (yearText != null)
        {
            yearText.text = unknownYear;
        }

        if (playableText != null)
        {
            playableText.text = unknownPlayable;
        }

        if (descriptionText != null)
        {
            descriptionText.text = unknownDescription;
        }

        if (coverImage != null)
        {
            coverImage.sprite = null;
            coverImage.enabled = false;
        }
    }

    private void SetPanelVisible(bool visible)
    {
        // Turns the whole metadata panel on or off in the scene.
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }
}
