using UnityEngine;

/**
 * Interface for a vinyl record that can be selected, inspected and played by the rest of our scripts.
 */
public interface IVinyl
{
    /**
     * Returns the record data of this vinyl.
     *
     * @return The RecordData ScriptableObject containing the meta information of the vinyl (e.g. title and tracks).
     */
    RecordData GetData();

    /**
     * Returns the transform of the whole vinyl (sleeve and disc) used for selection.
     *
     * @return The root transform of the vinyl.
     */
    Transform GetSelectionTransform();

    /**
     * Returns the transform of the vinyl disc itself.
     *
     * @return The transform of the disc inside the sleeve.
     */
    Transform GetVinylDiscTransform();
}
