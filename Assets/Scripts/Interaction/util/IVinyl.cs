using UnityEngine;

public interface IVinyl
{
    RecordData GetData();
    Transform GetSelectionTransform();
    Transform GetCoverTransform();
    Transform GetVinylDiscTransform();
    void OnPlaced();  
    void OnRemoved(); 
}
