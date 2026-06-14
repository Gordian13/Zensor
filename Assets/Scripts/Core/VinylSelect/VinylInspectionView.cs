using System.Collections.Generic;
using Core.VinylSelect;
using UnityEngine;

public class VinylInspectionView : MonoBehaviour
{
    [SerializeField] private VinylSelectController vinylSelectController;
    [SerializeField] private Transform inspectionPoint;
    [SerializeField] private float positionSpeed = 6f;
    [SerializeField] private float rotationSpeed = 8f;

    private readonly Dictionary<Transform, RestPose> restPoses = new();

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

        foreach (KeyValuePair<Transform, RestPose> vinyl in restPoses)
        {
            if (vinyl.Key == null)
                continue;

            if (ShouldShowAtInspectionPoint(vinyl.Key))
                MoveToInspectionPoint(vinyl.Key);
            else
                MoveToRestPose(vinyl.Key, vinyl.Value);
        }
    }

    private void CacheSelectedVinyl()
    {
        if (vinylSelectController?.SelectedVinyl == null)
            return;

        Transform selectedTransform = vinylSelectController.SelectedVinyl.GetSelectionTransform();
        if (selectedTransform == null || restPoses.ContainsKey(selectedTransform))
            return;

        restPoses.Add(selectedTransform, new RestPose(
            selectedTransform.localPosition,
            selectedTransform.localRotation
        ));
    }

    private bool ShouldShowAtInspectionPoint(Transform vinylTransform)
    {
        if (vinylSelectController == null ||
            vinylSelectController.SelectedVinyl == null ||
            inspectionPoint == null)
        {
            return false;
        }

        bool isSelectedVinyl =
            vinylSelectController.SelectedVinyl.GetSelectionTransform() == vinylTransform;

        bool usesInspectionPose =
            vinylSelectController.CurrentVinylState == VinylState.Selected ||
            vinylSelectController.CurrentVinylState == VinylState.InfoOpen;

        return isSelectedVinyl && usesInspectionPose;
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

    private readonly struct RestPose
    {
        public Vector3 LocalPosition { get; }
        public Quaternion LocalRotation { get; }

        public RestPose(Vector3 localPosition, Quaternion localRotation)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }
    }
}
