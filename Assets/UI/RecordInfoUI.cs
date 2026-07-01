using TMPro;
using UnityEngine;

// Displays the metadata of the currently selected record in the UI panel.
// This script only updates UI elements; it does not decide which record was selected.
public class RecordInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text authorText;
    [SerializeField] private TMP_Text albumText;
    [SerializeField] private TMP_Text yearText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Fallback Text")]
    [SerializeField] private string unknownTitle = "Unknown Title";
    [SerializeField] private string unknownAuthor = "Unknown Artist";
    [SerializeField] private string unknownAlbum = "Unknown Album";
    [SerializeField] private string unknownYear = "Unknown Year";
    [SerializeField] private string unknownDescription = "No description available.";

    // Used by the editor setup tool to connect the generated UI elements automatically.
    public void Configure(
        GameObject panel,
        TMP_Text title,
        TMP_Text author,
        TMP_Text album,
        TMP_Text year,
        TMP_Text description)
    {
        panelRoot = panel;
        titleText = title;
        authorText = author;
        albumText = album;
        yearText = year;
        descriptionText = description;
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

        if (albumText != null)
        {
            albumText.text = string.IsNullOrWhiteSpace(data.album) ? unknownAlbum : data.album;
        }

        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrWhiteSpace(data.description) ? unknownDescription : data.description;
        }

        if (yearText != null)
        {
            yearText.text = string.IsNullOrWhiteSpace(data.year) ? unknownYear : data.year;
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

        if (albumText != null)
        {
            albumText.text = unknownAlbum;
        }

        if (yearText != null)
        {
            yearText.text = unknownYear;
        }

        if (descriptionText != null)
        {
            descriptionText.text = unknownDescription;
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
