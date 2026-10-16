using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Info panel (inspection window) of the Fotowand. Shows the photo, title,
/// description and optional extra info of a <see cref="FotowandData"/> asset.
/// </summary>
/// <remarks>
/// While the panel is open, other interactions are blocked through
/// <see cref="GlobalInteractionState"/> and the HomeButton is disabled, so the
/// player cannot navigate away. <see cref="Core.FotowandSelect.FotowandBoxSelector"/>
/// additionally disables camera orbit and spot navigation.
/// </remarks>
public class FotowandUI : MonoBehaviour
{
    /// <summary>Root object of the panel; toggled on open/close.</summary>
    [Header("Panel Root")]
    public GameObject panel;

    /// <summary>Main photo image.</summary>
    [Header("Main Photo")]
    public Image photoImage;
    /// <summary>Title text.</summary>
    public TextMeshProUGUI titleText;
    /// <summary>Short description text.</summary>
    public TextMeshProUGUI descriptionText;

    /// <summary>Container of the optional extra info section; hidden if the data has no extra info.</summary>
    [Header("Extra Info Section")]
    public GameObject extraInfoSection;
    /// <summary>Heading of the extra info section.</summary>
    public TextMeshProUGUI extraInfoTitleText;
    /// <summary>Long-form text of the extra info section.</summary>
    public TextMeshProUGUI extraInfoContentText;
    /// <summary>Optional image of the extra info section.</summary>
    public Image extraInfoImage;

    /// <summary>Legacy "Press [E]" hint from the old proximity-based version. Optional.</summary>
    [Header("Interaction Hint")]
    public TextMeshProUGUI hintText;

    /// <summary>Button that closes the panel.</summary>
    [Header("Close Button")]
    public Button closeButton;

    // Found automatically by GameObject name, because the HomeButton lives in
    // the 'main' scene while this Canvas lives in the 'FW' scene
    // (cross-scene references cannot be assigned in the Inspector).
    private Button emergencyButton;

    /// <summary>True while the panel is visible.</summary>
    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        panel.SetActive(false);
        HideHint();

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        FindEmergencyButton();
    }

    private void FindEmergencyButton()
    {
        if (emergencyButton != null)
            return;

        GameObject homeButtonObj = GameObject.Find("HomeButton");
        if (homeButtonObj != null)
            emergencyButton = homeButtonObj.GetComponent<Button>();
    }

 
    /// <summary>
    /// Fills the panel with the given data, shows it and blocks other interactions.
    /// </summary>
    /// <param name="data">Content of the clicked poster.</param>
    public void Open(FotowandData data)
    {
        photoImage.sprite = data.photo;
        titleText.text = data.title;
        descriptionText.text = data.description;

        bool showExtra = data.hasExtraInfo;
        extraInfoSection.SetActive(showExtra);
        if (showExtra)
        {
            extraInfoTitleText.text = data.extraInfoTitle;
            extraInfoContentText.text = data.extraInfoContent;
            bool hasExtraImage = data.extraInfoImage != null;
            extraInfoImage.gameObject.SetActive(hasExtraImage);
            if (hasExtraImage)
                extraInfoImage.sprite = data.extraInfoImage;
        }

        HideHint();
        GlobalInteractionState.Instance.BlockInteractions();
        panel.SetActive(true);

        FindEmergencyButton();
        if (emergencyButton != null)
            emergencyButton.interactable = false;
    }

    /// <summary>
    /// Hides the panel and re-enables interactions and the HomeButton.
    /// </summary>
    public void Close()
    {
        GlobalInteractionState.Instance.UnblockInteractions();
        panel.SetActive(false);

        if (emergencyButton != null)
            emergencyButton.interactable = true;
    }

    /// <summary>Shows the legacy interaction hint. Not used by the current click-based flow.</summary>
    /// <param name="key">Key shown in the hint text.</param>
    public void ShowHint(KeyCode key)
    {
        if (hintText == null) return;
        hintText.text = $"Press [{key}] to interact";
        hintText.gameObject.SetActive(true);
    }

    /// <summary>Hides the legacy interaction hint.</summary>
    public void HideHint()
    {
        if (hintText == null) return;
        hintText.gameObject.SetActive(false);
    }
}