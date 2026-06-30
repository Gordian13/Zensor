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
    private static readonly Vector2 PanelAnchor = new Vector2(1f, 0.5f);
    private static readonly Vector2 PanelPivot = new Vector2(1f, 0.5f);
    private static readonly Vector2 PanelPosition = new Vector2(-185.03503f, -28.144318f);
    private static readonly Vector2 PanelSize = new Vector2(748.7676f, 1001.6851f);

    // Adds the setup action to the Unity menu: Tools > Zensor > Setup Record Metadata UI.
    [MenuItem("Tools/Zensor/Setup Record Metadata UI")]
    public static void Setup()
    {
        // Create or reuse the main UI containers before adding the individual UI fields.
        var canvas = GetOrCreateCanvas();
        var panel = GetOrCreatePanel(canvas.transform);

        RemoveObsoleteChild(panel.transform, "PlayableText");
        RemoveObsoleteChild(panel.transform, "CoverImage");

        var titleText = GetOrCreateText(panel.transform, "TitleText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(-48f, 75.3654f), 30f, out _);
        var authorText = GetOrCreateText(panel.transform, "AuthorText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(-48f, 30f), 30f, out _);
        var albumText = GetOrCreateText(panel.transform, "AlbumText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(-48f, 30f), 30f, out _);
        var yearText = GetOrCreateText(panel.transform, "YearText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -310f), new Vector2(-48f, 34.6054f), 30f, out _);
        var descriptionText = GetOrCreateText(panel.transform, "DescriptionText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -560f), new Vector2(-48f, 400f), 33f, out _);
        descriptionText.alignment = TextAlignmentOptions.TopLeft;
        descriptionText.overflowMode = TextOverflowModes.Overflow;

        // Add the display script to the panel and connect all generated UI references.
        var infoUI = panel.GetComponent<RecordInfoUI>();
        if (infoUI == null)
        {
            infoUI = panel.AddComponent<RecordInfoUI>();
        }

        infoUI.Configure(panel.gameObject, titleText, authorText, albumText, yearText, descriptionText);

        // Connect the panel and existing buttons to the vinyl state controller.
        var vinylSelectController = Object.FindFirstObjectByType<VinylSelectController>();
        if (vinylSelectController != null)
        {
            var selectionUI = canvas.GetComponent<VinylSelectionUI>();
            if (selectionUI == null)
            {
                selectionUI = canvas.gameObject.AddComponent<VinylSelectionUI>();
            }

            selectionUI.Configure(
                vinylSelectController,
                infoUI,
                FindDirectChild(canvas.transform, "Info"),
                FindDirectChild(canvas.transform, "CloseInfo"),
                FindDirectChild(canvas.transform, "BrowseMore"),
                FindDirectChild(canvas.transform, "Play"));
        }
        else
        {
            Debug.LogWarning("RecordMetadataUISetup: No VinylSelectController found in the scene.");
        }

        // Mark the scene as changed so Unity knows it should be saved.
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = panel.gameObject;
        Debug.Log("Record metadata UI setup complete.");
    }

    private static GameObject FindDirectChild(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        return child != null ? child.gameObject : null;
    }

    private static void RemoveObsoleteChild(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child != null)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }

    // Creates the Canvas that contains UI elements, or reuses an existing one.
    private static Canvas GetOrCreateCanvas()
    {
        var existingObject = GameObject.Find(CanvasName);
        if (existingObject != null && existingObject.TryGetComponent(out Canvas existingCanvas))
        {
            ConfigureCanvas(existingCanvas);
            return existingCanvas;
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
            ConfigurePanel(panelTransform.gameObject);
            return panelTransform.gameObject;
        }

        var panelGo = new GameObject(PanelName);
        panelGo.transform.SetParent(canvasTransform, false);
        ConfigurePanel(panelGo);

        return panelGo;
    }

    private static void ConfigurePanel(GameObject panelGo)
    {
        var rect = panelGo.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = panelGo.AddComponent<RectTransform>();
        }

        rect.anchorMin = PanelAnchor;
        rect.anchorMax = PanelAnchor;
        rect.pivot = PanelPivot;
        rect.anchoredPosition = PanelPosition;
        rect.sizeDelta = PanelSize;

        var image = panelGo.GetComponent<Image>();
        if (image == null)
        {
            image = panelGo.AddComponent<Image>();
        }

        image.color = new Color(0f, 0f, 0f, 0.65f);
    }

    // Creates or updates a TextMeshPro text field used by the metadata panel.
    private static TMP_Text GetOrCreateText(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
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

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var text = textGo.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textGo.AddComponent<TextMeshProUGUI>();
            created = true;
        }

        if (created)
        {
            text.text = name;

            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }
        }

        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        return text;
    }

}
