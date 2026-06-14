using UnityEngine;

public interface IVinyl
{
    RecordData GetData();
    Transform GetSelectionTransform();
    void OnPlaced();  
    void OnRemoved(); 
}
