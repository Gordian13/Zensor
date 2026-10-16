using record;
using UnityEngine;

/**
* @brief Applies the cover and label textures from a RecordData ScriptableObject
* to the corresponding parts of a vinyl record model.
*
* The component is used together with a RecordInteractionScript and a vinyl FBX
* model. The required child renderers are found automatically by their names.
*
* RecordData contains the individual textures for the front/back cover and
* front/back label, allowing different vinyl records to share the same 3D model
* while displaying different artwork.
*
* @see RecordData
* @see RecordInteractionScript
*/
public class CoverAndLabelChange : MonoBehaviour
{
    private RecordData _vinylData;

    private Renderer _labelFrontRenderer;
    private Renderer _labelBackRenderer;
    private Renderer _coverFrontRenderer;
    private Renderer _coverBackRenderer;

    private const string BaseMap = "_BaseMap";
    private const string MainTex = "_MainTex";

    /**
     * @brief Finds the required renderers and retrieves the RecordData.
     */
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

    /**
     * @brief Applies the textures from the current RecordData.
     */
    void Start()
    {
        if (_vinylData == null)
        {
            Debug.LogWarning($"{nameof(CoverAndLabelChange)} has no {nameof(RecordData)} assigned.", this);
            return;
        }

        ApplyTextures();
    }

    /**
     * @brief Finds the renderers of the cover and label objects in the hierarchy.
     *
     * The child objects must be named "LabelFront", "LabelBack", "CoverFront" and "CoverBack".
     */
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

    /**
     * @brief Replaces the current RecordData and updates the displayed textures.
     *
     * @param data The RecordData containing the new vinyl information and textures.
     */
    public void SetData(RecordData data)
    {
        _vinylData = data;
        if (_vinylData != null)
            ApplyTextures();
    }

    /**
     * @brief Applies the four textures stored in RecordData to their renderers.
     */
    void ApplyTextures()
    {
        ApplyToRenderer(_labelFrontRenderer, _vinylData.labelFrontTexture);
        ApplyToRenderer(_labelBackRenderer, _vinylData.labelBackTexture);
        ApplyToRenderer(_coverFrontRenderer, _vinylData.coverFrontTexture);
        ApplyToRenderer(_coverBackRenderer, _vinylData.coverBackTexture);
    }

    /**
     * @brief Applies a texture to a renderer's material.
     *
     * Supports both the URP "_BaseMap" and the standard shader "_MainTex"
     * material properties. An individual material instance is used so that
     * different vinyl objects can display different textures.
     *
     * @param rend Renderer that receives the texture.
     * @param tex Texture to apply.
     */
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
