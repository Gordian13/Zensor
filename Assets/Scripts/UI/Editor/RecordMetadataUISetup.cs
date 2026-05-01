using System.Collections.Generic;
using System.Linq;
using TMPro;
using record;
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

        var titleText = GetOrCreateText(panel.transform, "TitleText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -16f), new Vector2(-160f, -56f), 30);
        var authorText = GetOrCreateText(panel.transform, "AuthorText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -64f), new Vector2(-160f, -94f), 20);
        var yearText = GetOrCreateText(panel.transform, "YearText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -102f), new Vector2(-300f, -132f), 18);
        var playableText = GetOrCreateText(panel.transform, "PlayableText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(270f, -120f), new Vector2(-160f, -150f), 18);
        var descriptionText = GetOrCreateText(panel.transform, "DescriptionText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 16f), new Vector2(-16f, -168f), 18);
        descriptionText.alignment = TextAlignmentOptions.TopLeft;
        descriptionText.enableWordWrapping = true;
        descriptionText.overflowMode = TextOverflowModes.Overflow;

        var coverImage = GetOrCreateImage(panel.transform, "CoverImage", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-144f, -16f), new Vector2(-16f, -144f));
        coverImage.preserveAspect = true;

        var infoUI = panel.GetComponent<RecordInfoUI>();
        if (infoUI == null)
        {
            infoUI = panel.AddComponent<RecordInfoUI>();
        }

        infoUI.Configure(panel.gameObject, titleText, authorText, yearText, playableText, descriptionText, coverImage);

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

    [MenuItem("Tools/Zensor/Assign firstRecord* To Existing Vinyls")]
    public static void AssignFirstRecordAssetsToVinyls()
    {
        var vinyls = Object.FindObjectsByType<RecordInteractionSkript>(FindObjectsSortMode.None)
            .OrderBy(v => v.transform.position.x)
            .ToList();

        if (vinyls.Count == 0)
        {
            Debug.LogWarning("RecordMetadataUISetup: No vinyls with RecordInteractionSkript found in scene.");
            return;
        }

        var recordAssets = LoadFirstRecordAssets();
        if (recordAssets.Count == 0)
        {
            Debug.LogWarning("RecordMetadataUISetup: No RecordData assets found in Assets/Scripts/record.");
            return;
        }

        for (int i = 0; i < vinyls.Count; i++)
        {
            vinyls[i].data = recordAssets[i % recordAssets.Count];
            EditorUtility.SetDirty(vinyls[i]);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"Assigned {recordAssets.Count} record assets across {vinyls.Count} vinyl object(s).");
    }

    [MenuItem("Tools/Zensor/Spawn Vinyl Test Set From firstRecord*")]
    public static void SpawnVinylTestSet()
    {
        var template = Object.FindFirstObjectByType<RecordInteractionSkript>();
        if (template == null)
        {
            Debug.LogWarning("RecordMetadataUISetup: No RecordInteractionSkript found to use as template.");
            return;
        }

        var recordAssets = LoadFirstRecordAssets();
        if (recordAssets.Count == 0)
        {
            Debug.LogWarning("RecordMetadataUISetup: No RecordData assets found in Assets/Scripts/record.");
            return;
        }

        const float spacing = 0.45f;
        var startPosition = template.transform.position;
        var parent = template.transform.parent;

        for (int i = 0; i < recordAssets.Count; i++)
        {
            GameObject vinylObject;
            RecordInteractionSkript interaction;

            if (i == 0)
            {
                vinylObject = template.gameObject;
                interaction = template;
            }
            else
            {
                vinylObject = Object.Instantiate(template.gameObject, parent);
                interaction = vinylObject.GetComponent<RecordInteractionSkript>();
            }

            vinylObject.name = $"SM_VinylDisc_{i + 1}";
            vinylObject.transform.position = startPosition + new Vector3(spacing * i, 0f, 0f);
            vinylObject.transform.rotation = template.transform.rotation;

            if (vinylObject.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            interaction.data = recordAssets[i];
            EditorUtility.SetDirty(interaction);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"Spawned/assigned {recordAssets.Count} vinyls for multi-record testing.");
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
        rect.sizeDelta = new Vector2(620f, 280f);

        var image = panelGo.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.65f);

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
        text.color = new Color(0.95f, 0.95f, 0.95f, 1f);

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

    private static List<RecordData> LoadFirstRecordAssets()
    {
        const string searchFolder = "Assets/Scripts/record";
        var guids = AssetDatabase.FindAssets("t:RecordData firstRecord", new[] { searchFolder });
        var assets = new List<RecordData>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<RecordData>(path);
            if (data != null)
            {
                assets.Add(data);
            }
        }

        return assets.OrderBy(a => a.name).ToList();
    }
}
