using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the UI panel that appears when a player interacts with a Fotowand.
/// There should be 'one' instance of this in the scene, shared by all Fotowand objects.
/// 
/// Setup:
///     1. Create a Canvas with a panel containing the fields below
///     2. Assign all UI references in the Inspector
///     3. Drag this component into the "UI" field of every FotowandInteractable
/// </summary>

public class FotowandUI : MonoBehaviour
{
    [Header("Panel Root")]
    [Tooltip("The root GameObject of the entire UI panel - toggled on/off")]
    public GameObject panel;

    [Header("Main Photo")]
    public Image photoImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Extra Info Section")]
    [Tooltip("Parent object of the extra info block - hidden when not needed")]
    public GameObject extraInfoSection;
    public TextMeshProUGUI extraInfoTitleText;
    public TextMeshProUGUI extraInfoContentText;
    public Image extraInfoImage;

    [Header("Interaction Hint")]
    [Tooltip("Small hint label shown to the player (e.g. 'Press E to interact')")]
    public TextMeshProUGUI hintText;

    [Header("Close Button")]
    public Button closeButton;

    // -- Unity lifecycle --
    private void Awake()
    {
        panel.SetActive(false);
        HideHint();

        if (closeButton != null)
        
            closeButton.onClick.AddListener(Close);
        

    }

    private void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    // -- Public API --
    /// <summary>
    /// Opens the UI panel and populates it with data from the given FotowandData asset.
    /// </summary>
    
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

            bool hasExtraImage = data.extraInfoImage!= null;
            extraInfoImage.gameObject.SetActive(hasExtraImage);
            if (hasExtraImage)
                extraInfoImage.sprite = data.extraInfoImage;

        }
        HideHint();
        panel.SetActive(true);
    }

    /// <summary>
    /// Closes the UI panel
    /// </summary>

    public void Close()
    {
        panel.SetActive(false);

    }

    /// <summary>
    /// Shows the interaction hint label with the correct key name.
    /// </summary>
     
    public void ShowHint(KeyCode key)
    {
        if (hintText == null) return;
        hintText.text = $"Press [{key}] to interact";
        hintText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the interaction hint label.
    /// </summary>

    public void HideHint()
    {
        if (hintText ==null) return;
        hintText.gameObject.SetActive(false);
    }

   
}
