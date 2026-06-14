using Core.VinylSelect;
using UnityEngine;

/**
 * Moves the selected vinyl to the inspection point and returns it to its original pose.
 * Also controls whether the disc sits inside the cover or peeks out of it.
 */
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

    /**
     * Validates the references required to display a vinyl at the inspection point.
     */
    private void Awake()
    {
        if (vinylSelectController == null)
            Debug.LogError($"{nameof(VinylInspectionView)} has no VinylSelectController assigned.", this);

        if (inspectionPoint == null)
            Debug.LogError($"{nameof(VinylInspectionView)} has no inspection point assigned.", this);
    }

    /**
     * Updates the selected vinyl and disc positions according to the current vinyl state.
     */
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
            else if (ShouldDiscSitInCover())
                MoveDiscToRestPose(_restPose);
        }
        else
        {
            MoveToRestPose(_inspectedVinyl, _restPose);
            MoveDiscToRestPose(_restPose);
        }
    }

    /**
     * Stores the original pose of a newly selected vinyl and its disc.
     */
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

    /**
     * Returns true while the selected vinyl should remain in front of the camera.
     */
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
            vinylSelectController.CurrentVinylState != VinylState.BrowsingBox;

        return isSelectedVinyl && usesInspectionPose;
    }

    /**
     * Returns true while the disc should peek out of its cover.
     */
    private bool ShouldDiscPeekOut()
    {
        return vinylSelectController != null &&
               vinylSelectController.CurrentVinylState == VinylState.VinylSelected;
    }

    /**
     * Returns true while the info panel is open and the disc should sit inside its cover.
     */
    private bool ShouldDiscSitInCover()
    {
        return vinylSelectController != null &&
               vinylSelectController.CurrentVinylState == VinylState.VinylInfoOpen;
    }

    /**
     * Immediately restores the previously inspected vinyl before another vinyl is cached.
     */
    private void RestoreCurrentVinylImmediately()
    {
        if (!_hasRestPose || _inspectedVinyl == null)
            return;

        _inspectedVinyl.localPosition = _restPose.LocalPosition;
        _inspectedVinyl.localRotation = _restPose.LocalRotation;

        if (_restPose.VinylDisc != null)
            _restPose.VinylDisc.localPosition = _restPose.VinylDiscLocalPosition;
    }

    /**
     * Smoothly moves and rotates the complete vinyl asset to the inspection point.
     */
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

    /**
     * Smoothly returns the complete vinyl asset to its stored local pose.
     */
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

    /**
     * Moves the disc out of its cover by the configured local X offset.
     */
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

    /**
     * Moves the disc back to its original local position inside the cover.
     */
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

    /**
     * Stores the original local pose of the complete vinyl and its disc.
     */
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
