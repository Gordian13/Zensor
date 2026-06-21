using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Unity editor helper that creates and wires the record metadata UI automatically.
// This runs only inside the Unity Editor and is not part of the in-game runtime logic.
public static class RecordMetadataUISetup
{
    private const string CanvasName = "RecordInfoCanvas";
    private const string PanelName = "RecordInfoPanel";
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    // Adds the setup action to the Unity menu: Tools > Zensor > Setup Record Metadata UI.
    [MenuItem("Tools/Zensor/Setup Record Metadata UI")]
    public static void Setup()
    {
        // Create or reuse the main UI containers before adding the individual UI fields.
        var canvas = GetOrCreateCanvas();
        var panel = GetOrCreatePanel(canvas.transform);

        var titleText = GetOrCreateText(panel.transform, "TitleText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -16f), new Vector2(-160f, -56f), 30, out _);
        var authorText = GetOrCreateText(panel.transform, "AuthorText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -64f), new Vector2(-160f, -94f), 20, out _);
        var yearText = GetOrCreateText(panel.transform, "YearText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -102f), new Vector2(-300f, -132f), 18, out _);
        var playableText = GetOrCreateText(panel.transform, "PlayableText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(270f, -120f), new Vector2(-160f, -150f), 18, out _);
        var descriptionText = GetOrCreateText(panel.transform, "DescriptionText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 16f), new Vector2(-16f, -168f), 18, out var descriptionCreated);
        if (descriptionCreated)
        {
            descriptionText.alignment = TextAlignmentOptions.TopLeft;
            // descriptionText.enableWordWrapping = true;
            descriptionText.overflowMode = TextOverflowModes.Overflow;
        }

        var coverImage = GetOrCreateImage(panel.transform, "CoverImage", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-144f, -16f), new Vector2(-16f, -144f), out var coverCreated);
        if (coverCreated)
        {
            coverImage.preserveAspect = true;
        }

        // Add the display script to the panel and connect all generated UI references.
        var infoUI = panel.GetComponent<RecordInfoUI>();
        if (infoUI == null)
        {
            infoUI = panel.AddComponent<RecordInfoUI>();
        }

        infoUI.Configure(panel.gameObject, titleText, authorText, yearText, playableText, descriptionText, coverImage);

        // Connect the click/selection controller to the scene camera.
        var camera = Camera.main;
        if (camera == null)
        {
            camera = Object.FindFirstObjectByType<Camera>();
        }

        if (camera != null)
        {
            var selectionController = camera.GetComponent<RecordSelectionController>();
            if (selectionController == null)
            {
                selectionController = camera.gameObject.AddComponent<RecordSelectionController>();
            }

            selectionController.Configure(camera, infoUI);
        }
        else
        {
            Debug.LogWarning("RecordMetadataUISetup: No camera found. Add RecordSelectionController manually after creating a camera.");
        }

        // Mark the scene as changed so Unity knows it should be saved.
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = panel.gameObject;
        Debug.Log("Record metadata UI setup complete.");
    }

    // Creates the Canvas that contains UI elements, or reuses an existing one.
    private static Canvas GetOrCreateCanvas()
    {
        var existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null)
        {
            ConfigureCanvas(existing);
            return existing;
        }

        var canvasGo = new GameObject(CanvasName);
        var canvas = canvasGo.AddComponent<Canvas>();
        ConfigureCanvas(canvas);
        return canvas;
    }

    // Keeps the UI proportional across Free Aspect, Full HD, and other screen resolutions.
    private static void ConfigureCanvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    // Creates the dark metadata background panel, or reuses it if it already exists.
    private static GameObject GetOrCreatePanel(Transform canvasTransform)
    {
        var panelTransform = canvasTransform.Find(PanelName);
        if (panelTransform != null)
        {
            return panelTransform.gameObject;
        }

        var panelGo = new GameObject(PanelName);
        panelGo.transform.SetParent(canvasTransform, false);

        var rect = panelGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(16f, 16f);
        rect.sizeDelta = new Vector2(620f, 280f);

        var image = panelGo.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.65f);

        return panelGo;
    }

    // Creates or updates a TextMeshPro text field used by the metadata panel.
    private static TMP_Text GetOrCreateText(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        float fontSize,
        out bool created)
    {
        // If the text object already exists, reuse it instead of creating duplicates.
        var existing = parent.Find(name);
        created = existing == null;
        var textGo = existing != null ? existing.gameObject : new GameObject(name);

        if (created)
        {
            textGo.transform.SetParent(parent, false);
        }

        var rect = textGo.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = textGo.AddComponent<RectTransform>();
        }

        if (created)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        var text = textGo.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textGo.AddComponent<TextMeshProUGUI>();
            created = true;
        }

        if (created)
        {
            text.text = name;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }
        }

        return text;
    }

    // Creates or updates the image field used for the record cover.
    private static Image GetOrCreateImage(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        out bool created)
    {
        // If the image object already exists, reuse it instead of creating duplicates.
        var existing = parent.Find(name);
        created = existing == null;
        var imageGo = existing != null ? existing.gameObject : new GameObject(name);

        if (created)
        {
            imageGo.transform.SetParent(parent, false);
        }

        var rect = imageGo.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = imageGo.AddComponent<RectTransform>();
        }

        if (created)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        var image = imageGo.GetComponent<Image>();
        if (image == null)
        {
            image = imageGo.AddComponent<Image>();
            created = true;
        }

        if (created)
        {
            image.color = Color.white;
        }

        return image;
    }

}
