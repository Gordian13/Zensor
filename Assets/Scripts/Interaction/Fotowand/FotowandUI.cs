using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class FotowandUI : MonoBehaviour
{
    [Header("Panel Root")]
    public GameObject panel;

    [Header("Main Photo")]
    public Image photoImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Extra Info Section")]
    public GameObject extraInfoSection;
    public TextMeshProUGUI extraInfoTitleText;
    public TextMeshProUGUI extraInfoContentText;
    public Image extraInfoImage;

    [Header("Interaction Hint")]
    public TextMeshProUGUI hintText;

    [Header("Close Button")]
    public Button closeButton;

    // Dicari otomatis lewat nama GameObject, karena HomeButton berada
    // di scene 'main' sedangkan Canvas ini di scene 'FW' (cross-scene
    // reference tidak bisa di-drag manual di Inspector).
    private Button emergencyButton;

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

    private void Update()
    {
    }

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

    public void Close()
    {
        GlobalInteractionState.Instance.UnblockInteractions();
        panel.SetActive(false);

        if (emergencyButton != null)
            emergencyButton.interactable = true;
    }

    public void ShowHint(KeyCode key)
    {
        if (hintText == null) return;
        hintText.text = $"Press [{key}] to interact";
        hintText.gameObject.SetActive(true);
    }

    public void HideHint()
    {
        if (hintText == null) return;
        hintText.gameObject.SetActive(false);
    }
}