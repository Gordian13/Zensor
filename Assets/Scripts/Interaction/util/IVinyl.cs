using UnityEngine;

public interface IVinyl
{
    RecordData GetData();
    Transform GetSelectionTransform();
    Transform GetVinylDiscTransform();
}
