using TMPro;
using UnityEngine;

/**
 * @brief Displays the selected record's metadata in a TextMesh Pro panel.
 *
 * Assign the panel root and text fields in the Inspector or through Configure().
 * Awake hides the panel and uses this GameObject if no panel root was assigned.
 * Missing text references are skipped; missing data uses the configured fallback text.
 * This component only presents data. VinylSelectionUI controls visibility for the
 * selection and player information states; RecordBoxSelector can also display hover data.
 * Edit record contents in the RecordData assets, not in this component.
 * @see RecordMetadataUISetup
 */
public class RecordInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text authorText;
    [SerializeField] private TMP_Text albumText;
    [SerializeField] private TMP_Text yearText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Fallback Text")]
    [SerializeField] private string unknownTitle = "Unbekannter Titel";
    [SerializeField] private string unknownAuthor = "Unbekannter Künstler";
    [SerializeField] private string unknownAlbum = "Unbekanntes Album";
    [SerializeField] private string unknownYear = "Unbekanntes Jahr";
    [SerializeField] private string unknownDescription = "Keine Beschreibung verfügbar.";

    /**
     * @brief Assigns UI references without changing the displayed data or visibility.
     * @param panel GameObject activated or deactivated when showing or hiding the panel.
     * @param title Text field for the record title.
     * @param author Text field for the artist or author.
     * @param album Text field for the album name.
     * @param year Text field for the release year.
     * @param description Text field for the record's background information.
     */
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

    /**
     * @brief Populates the assigned fields and shows the panel.
     * @param data Record metadata to display; null shows only fallback values.
     *
     * Empty or whitespace-only fields use their individual fallback strings.
     * Displaying another record replaces the previous values.
     */
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

    /** @brief Hides the panel without clearing its text or changing the selection state. */
    public void Hide()
    {
        SetPanelVisible(false);
    }

    /** @brief Replaces all assigned text fields with fallbacks without changing visibility. */
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
