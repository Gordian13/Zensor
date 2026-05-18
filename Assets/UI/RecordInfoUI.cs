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

        ApplyResponsiveLayout();
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

        ApplyResponsiveLayout();
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

    private void ApplyResponsiveLayout()
    {
        if (panelRoot == null)
        {
            return;
        }

        var panelRect = panelRoot.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            return;
        }

        var parentRect = panelRect.parent as RectTransform;
        var parentHeight = parentRect != null ? parentRect.rect.height : Screen.height;
        var panelHeight = Mathf.Clamp(parentHeight * 0.45f, 150f, 280f);

        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.offsetMin = new Vector2(12f, 12f);
        panelRect.offsetMax = new Vector2(-12f, 12f + panelHeight);

        var coverWidth = coverImage != null && coverImage.enabled ? 126f : 0f;
        var rightPadding = 16f + coverWidth;

        SetTopTextRect(titleText, 16f, rightPadding, 12f, 34f, 22f);
        SetTopTextRect(authorText, 16f, rightPadding, 42f, 24f, 17f);
        SetTopTextRect(yearText, 16f, 260f, 68f, 22f, 15f);
        SetTopTextRect(playableText, 150f, rightPadding, 68f, 22f, 15f);
        SetStretchTextRect(descriptionText, 16f, 16f, 12f, 96f, 15f);

        if (coverImage != null)
        {
            var coverRect = coverImage.rectTransform;
            coverRect.anchorMin = new Vector2(1f, 1f);
            coverRect.anchorMax = new Vector2(1f, 1f);
            coverRect.pivot = new Vector2(1f, 1f);
            coverRect.anchoredPosition = new Vector2(-16f, -16f);
            coverRect.sizeDelta = new Vector2(118f, 118f);
            coverImage.preserveAspect = true;
        }
    }

    private static void SetTopTextRect(TMP_Text text, float left, float right, float top, float height, float fontSize)
    {
        if (text == null)
        {
            return;
        }

        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(left, -top - height);
        rect.offsetMax = new Vector2(-right, -top);

        text.fontSize = fontSize;
        text.textWrappingMode = TextWrappingModes.Normal;
    }

    private static void SetStretchTextRect(TMP_Text text, float left, float right, float bottom, float top, float fontSize)
    {
        if (text == null)
        {
            return;
        }

        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);

        text.fontSize = fontSize;
        text.textWrappingMode = TextWrappingModes.Normal;
    }
}
