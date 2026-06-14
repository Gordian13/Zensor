using Core.VinylSelect;
using UnityEngine;

public class VinylInspectionView : MonoBehaviour
{
    [SerializeField] private VinylSelectController vinylSelectController;
    [SerializeField] private Transform inspectionPoint;
    [SerializeField] private float positionSpeed = 6f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float vinylDiscInspectionXOffset = 0.2f;

    private Transform _inspectedVinyl;
    private RestPose _restPose;
    private bool _hasRestPose;

    private void Awake()
    {
        if (vinylSelectController == null)
            Debug.LogError($"{nameof(VinylInspectionView)} has no VinylSelectController assigned.", this);

        if (inspectionPoint == null)
            Debug.LogError($"{nameof(VinylInspectionView)} has no inspection point assigned.", this);
    }

    private void Update()
    {
        CacheSelectedVinyl();

        if (!_hasRestPose || _inspectedVinyl == null)
            return;

        if (ShouldShowAtInspectionPoint())
        {
            MoveToInspectionPoint(_inspectedVinyl);

            if (ShouldDiscPeekOut())
                MoveDiscToInspectionPose(_restPose);
            else
                MoveDiscToRestPose(_restPose);
        }
        else
        {
            MoveToRestPose(_inspectedVinyl, _restPose);
            MoveDiscToRestPose(_restPose);
        }
    }

    private void CacheSelectedVinyl()
    {
        if (vinylSelectController?.SelectedVinyl == null)
            return;

        Transform selectedTransform = vinylSelectController.SelectedVinyl.GetSelectionTransform();
        if (selectedTransform == null || selectedTransform == _inspectedVinyl)
            return;

        RestoreCurrentVinylImmediately();

        Transform vinylDisc = vinylSelectController.SelectedVinyl.GetVinylDiscTransform();

        _inspectedVinyl = selectedTransform;
        _restPose = new RestPose(
            selectedTransform.localPosition,
            selectedTransform.localRotation,
            vinylDisc,
            vinylDisc != null ? vinylDisc.localPosition : Vector3.zero
        );
        _hasRestPose = true;
    }

    private bool ShouldShowAtInspectionPoint()
    {
        if (vinylSelectController == null ||
            vinylSelectController.SelectedVinyl == null ||
            inspectionPoint == null)
        {
            return false;
        }

        bool isSelectedVinyl =
            vinylSelectController.SelectedVinyl.GetSelectionTransform() == _inspectedVinyl;

        bool usesInspectionPose =
            vinylSelectController.CurrentVinylState == VinylState.Selected ||
            vinylSelectController.CurrentVinylState == VinylState.InfoOpen;

        return isSelectedVinyl && usesInspectionPose;
    }

    private bool ShouldDiscPeekOut()
    {
        return vinylSelectController != null &&
               vinylSelectController.CurrentVinylState == VinylState.Selected;
    }

    private void RestoreCurrentVinylImmediately()
    {
        if (!_hasRestPose || _inspectedVinyl == null)
            return;

        _inspectedVinyl.localPosition = _restPose.LocalPosition;
        _inspectedVinyl.localRotation = _restPose.LocalRotation;

        if (_restPose.VinylDisc != null)
            _restPose.VinylDisc.localPosition = _restPose.VinylDiscLocalPosition;
    }

    private void MoveToInspectionPoint(Transform vinylTransform)
    {
        vinylTransform.position = Vector3.Lerp(
            vinylTransform.position,
            inspectionPoint.position,
            positionSpeed * Time.deltaTime
        );

        vinylTransform.rotation = Quaternion.Slerp(
            vinylTransform.rotation,
            inspectionPoint.rotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void MoveToRestPose(Transform vinylTransform, RestPose restPose)
    {
        vinylTransform.localPosition = Vector3.Lerp(
            vinylTransform.localPosition,
            restPose.LocalPosition,
            positionSpeed * Time.deltaTime
        );

        vinylTransform.localRotation = Quaternion.Slerp(
            vinylTransform.localRotation,
            restPose.LocalRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void MoveDiscToInspectionPose(RestPose restPose)
    {
        if (restPose.VinylDisc == null)
            return;

        Vector3 targetPosition =
            restPose.VinylDiscLocalPosition + Vector3.left * vinylDiscInspectionXOffset;

        restPose.VinylDisc.localPosition = Vector3.Lerp(
            restPose.VinylDisc.localPosition,
            targetPosition,
            positionSpeed * Time.deltaTime
        );
    }

    private void MoveDiscToRestPose(RestPose restPose)
    {
        if (restPose.VinylDisc == null)
            return;

        restPose.VinylDisc.localPosition = Vector3.Lerp(
            restPose.VinylDisc.localPosition,
            restPose.VinylDiscLocalPosition,
            positionSpeed * Time.deltaTime
        );
    }

    private readonly struct RestPose
    {
        public Vector3 LocalPosition { get; }
        public Quaternion LocalRotation { get; }
        public Transform VinylDisc { get; }
        public Vector3 VinylDiscLocalPosition { get; }

        public RestPose(
            Vector3 localPosition,
            Quaternion localRotation,
            Transform vinylDisc,
            Vector3 vinylDiscLocalPosition)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            VinylDisc = vinylDisc;
            VinylDiscLocalPosition = vinylDiscLocalPosition;
        }
    }
}
