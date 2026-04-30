using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text authorText;
    [SerializeField] private TMP_Text yearText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image coverImage;

    [Header("Fallback Text")]
    [SerializeField] private string unknownTitle = "Unknown Title";
    [SerializeField] private string unknownAuthor = "Unknown Artist";
    [SerializeField] private string unknownYear = "Unknown Year";
    [SerializeField] private string unknownDescription = "No description available.";

    public void Configure(
        GameObject panel,
        TMP_Text title,
        TMP_Text author,
        TMP_Text year,
        TMP_Text description,
        Image cover)
    {
        panelRoot = panel;
        titleText = title;
        authorText = author;
        yearText = year;
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

    public void ShowData(RecordData data)
    {
        if (data == null)
        {
            Clear();
            SetPanelVisible(true);
            return;
        }

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(data.title) ? unknownTitle : data.title;
        }

        if (authorText != null)
        {
            authorText.text = string.IsNullOrWhiteSpace(data.author) ? unknownAuthor : data.author;
        }

        if (yearText != null)
        {
            yearText.text = data.year > 0 ? data.year.ToString() : unknownYear;
        }

        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrWhiteSpace(data.description) ? unknownDescription : data.description;
        }

        if (coverImage != null)
        {
            coverImage.sprite = data.sprite;
            coverImage.enabled = data.sprite != null;
        }

        SetPanelVisible(true);
    }

    public void Hide()
    {
        SetPanelVisible(false);
    }

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
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }
}
