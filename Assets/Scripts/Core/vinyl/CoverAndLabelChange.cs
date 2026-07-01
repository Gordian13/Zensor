using record;
using UnityEngine;

public class CoverAndLabelChange : MonoBehaviour
{
    private RecordData _vinylData;

    private Renderer _labelFrontRenderer;
    private Renderer _labelBackRenderer;
    private Renderer _coverFrontRenderer;
    private Renderer _coverBackRenderer;

    private const string BaseMap = "_BaseMap";
    private const string MainTex = "_MainTex";

    void Awake()
    {
        FindRenderers();
        RecordInteractionScript recordInteraction = GetComponent<RecordInteractionScript>();
        if (recordInteraction == null)
        {
            Debug.LogWarning($"{nameof(CoverAndLabelChange)} needs a {nameof(RecordInteractionScript)} on the same GameObject.", this);
            return;
        }

        _vinylData = recordInteraction.GetData();
    }

    void Start()
    {
        if (_vinylData == null)
        {
            Debug.LogWarning($"{nameof(CoverAndLabelChange)} has no {nameof(RecordData)} assigned.", this);
            return;
        }

        ApplyTextures();
    }

    void FindRenderers()
    {
        // assign specific child component's renderer to be able to display new image texture
        var renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
        {
            switch (r.name)
            {
                case "LabelFront":
                    _labelFrontRenderer = r;
                    break;

                case "LabelBack":
                    _labelBackRenderer = r;
                    break;

                case "CoverFront":
                    _coverFrontRenderer = r;
                    break;

                case "CoverBack":
                    _coverBackRenderer = r;
                    break;
            }
        }
    }

    public void SetData(RecordData data)
    {
        _vinylData = data;
        if (_vinylData != null)
            ApplyTextures();
    }

    void ApplyTextures()
    {
        ApplyToRenderer(_labelFrontRenderer, _vinylData.labelFrontTexture);
        ApplyToRenderer(_labelBackRenderer, _vinylData.labelBackTexture);
        ApplyToRenderer(_coverFrontRenderer, _vinylData.coverFrontTexture);
        ApplyToRenderer(_coverBackRenderer, _vinylData.coverBackTexture);
    }

    void ApplyToRenderer(Renderer rend, Texture2D tex)
    {
        if (rend == null || tex == null) return;

        // creating new instance of material so that vinyls can have individual labels
        Material matInstance = rend.material;

        // support for URP + standard shader
        if (matInstance.HasProperty(BaseMap))
            matInstance.SetTexture(BaseMap, tex);

        if (matInstance.HasProperty(MainTex))
            matInstance.SetTexture(MainTex, tex);
    }
}
