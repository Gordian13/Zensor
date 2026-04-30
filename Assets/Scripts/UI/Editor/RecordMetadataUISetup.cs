using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RecordMetadataUISetup
{
    private const string CanvasName = "RecordInfoCanvas";
    private const string PanelName = "RecordInfoPanel";

    [MenuItem("Tools/Zensor/Setup Record Metadata UI")]
    public static void Setup()
    {
        var canvas = GetOrCreateCanvas();
        var panel = GetOrCreatePanel(canvas.transform);

        var titleText = GetOrCreateText(panel.transform, "TitleText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -16f), new Vector2(-140f, -56f), 28);
        var authorText = GetOrCreateText(panel.transform, "AuthorText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -64f), new Vector2(-140f, -96f), 20);
        var yearText = GetOrCreateText(panel.transform, "YearText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -102f), new Vector2(-140f, -132f), 18);
        var descriptionText = GetOrCreateText(panel.transform, "DescriptionText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 16f), new Vector2(-16f, -142f), 18);
        descriptionText.alignment = TextAlignmentOptions.TopLeft;
        descriptionText.enableWordWrapping = true;
        descriptionText.overflowMode = TextOverflowModes.Overflow;

        var coverImage = GetOrCreateImage(panel.transform, "CoverImage", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-124f, -16f), new Vector2(-16f, -124f));
        coverImage.preserveAspect = true;

        var infoUI = panel.GetComponent<RecordInfoUI>();
        if (infoUI == null)
        {
            infoUI = panel.AddComponent<RecordInfoUI>();
        }

        infoUI.Configure(panel.gameObject, titleText, authorText, yearText, descriptionText, coverImage);

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

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = panel.gameObject;
        Debug.Log("Record metadata UI setup complete.");
    }

    private static Canvas GetOrCreateCanvas()
    {
        var existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null)
        {
            return existing;
        }

        var canvasGo = new GameObject(CanvasName);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        return canvas;
    }

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
        rect.sizeDelta = new Vector2(540f, 280f);

        var image = panelGo.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.75f);

        return panelGo;
    }

    private static TMP_Text GetOrCreateText(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        float fontSize)
    {
        var existing = parent.Find(name);
        var textGo = existing != null ? existing.gameObject : new GameObject(name);

        if (existing == null)
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
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var text = textGo.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textGo.AddComponent<TextMeshProUGUI>();
        }

        text.text = name;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.TopLeft;

        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        return text;
    }

    private static Image GetOrCreateImage(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        var existing = parent.Find(name);
        var imageGo = existing != null ? existing.gameObject : new GameObject(name);

        if (existing == null)
        {
            imageGo.transform.SetParent(parent, false);
        }

        var rect = imageGo.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = imageGo.AddComponent<RectTransform>();
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var image = imageGo.GetComponent<Image>();
        if (image == null)
        {
            image = imageGo.AddComponent<Image>();
        }

        image.color = Color.white;
        return image;
    }
}
