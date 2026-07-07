using UnityEngine;

public interface IFotowand
{
    FotowandData GetData();
    Transform GetSelectionTransform();
    void SetHighlight(bool isHighlighted);
}