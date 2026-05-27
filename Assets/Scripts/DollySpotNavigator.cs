using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DollySpotNavigator : MonoBehaviour
{
    [SerializeField] private CinemachineSplineDolly dolly;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float[] spots = { 0f, 0.25f, 0.5f, 0.75f, 1f };

    private Coroutine moveRoutine;
    private int currentSpotIndex;

    private void Awake()
    {
        if (dolly == null)
        {
            dolly = GetComponent<CinemachineSplineDolly>();
        }
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || spots.Length == 0)
        {
            return;
        }

        if (kb.digit1Key.wasPressedThisFrame) GoToSpot(0);
        if (kb.digit2Key.wasPressedThisFrame) GoToSpot(1);
        if (kb.digit3Key.wasPressedThisFrame) GoToSpot(2);
        if (kb.digit4Key.wasPressedThisFrame) GoToSpot(3);
        if (kb.digit5Key.wasPressedThisFrame) GoToSpot(4);

        if (kb.eKey.wasPressedThisFrame) GoToNextSpot();
        if (kb.qKey.wasPressedThisFrame) GoToPreviousSpot();
    }

    public void GoToNextSpot()
    {
        GoToSpot(currentSpotIndex + 1);
    }

    public void GoToPreviousSpot()
    {
        GoToSpot(currentSpotIndex - 1);
    }

    public void GoToSpot(int spotIndex)
    {
        if (dolly == null)
        {
            Debug.LogError($"{nameof(DollySpotNavigator)} needs a {nameof(CinemachineSplineDolly)} reference.", this);
            return;
        }

        if (spots.Length == 0)
        {
            return;
        }

        currentSpotIndex = Mathf.Clamp(spotIndex, 0, spots.Length - 1);

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveToPosition(spots[currentSpotIndex]));
    }

    private IEnumerator MoveToPosition(float targetPosition)
    {
        float startPosition = dolly.CameraPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            dolly.CameraPosition = Mathf.Lerp(startPosition, targetPosition, easing.Evaluate(t));
            yield return null;
        }

        dolly.CameraPosition = targetPosition;
        moveRoutine = null;
    }
}
