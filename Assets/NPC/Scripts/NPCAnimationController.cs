using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Updates the NPC animator from the current NavMesh movement.
/// It also turns the character toward its walking direction.
/// </summary>
/// <remarks>
/// Values saved on a prefab override the initial values written in this script.
/// Tune referenceWalkSpeed and animationSpeedMultiplier per character because different models and clips can have different stride lengths.
/// </remarks>
public class NPCAnimationController : MonoBehaviour
{
    /// <summary>
    /// Agent that supplies the real movement speed and desired walking direction.
    /// <see cref="Reset"/> and <see cref="Awake"/> try to find it on the same GameObject when the field is empty.
    /// </summary>
    [Tooltip("NavMeshAgent used as the source for movement speed and direction. Usually the agent on the same GameObject.")]
    [SerializeField] private NavMeshAgent agent;

    /// <summary>
    /// Animator that receives the Speed, MoveSpeed, RandomIdle and IsInteracting parameters.
    /// It may be placed on a child object because the animated character model is often below the NPC root.
    /// </summary>
    [Tooltip("Animator that contains the Speed, MoveSpeed, RandomIdle and IsInteracting parameters. A child Animator is found automatically when empty.")]
    [SerializeField] private Animator animator;

    /// <summary>
    /// World movement speed, in Unity units per second, that is treated as the reference speed of the walk clip.
    /// The current agent speed is divided by this value before it is sent to the MoveSpeed animator parameter.
    /// A value of 2 therefore produces a base playback factor of 1 when the agent moves at 2 units per second.
    /// Keep this value above zero. Start with the speed stored in the NPC's normal <see cref="NPCMoodData"/>, then tune the multiplier if the feet still slide.
    /// </summary>
    [Tooltip("Reference movement speed in units per second. actual agent speed / this value forms the base animation playback speed. Keep above zero.")]
    [SerializeField] private float referenceWalkSpeed = 2f;

    /// <summary>
    /// Final manual correction for the walk animation playback speed.
    /// The complete calculation is: actual agent speed / referenceWalkSpeed * animationSpeedMultiplier.
    /// Increase this value when the character moves farther than its feet appear to walk. Decrease it when the feet move too quickly for the covered distance.
    /// The default 1.5 is only a starting value because animation clips and character scales can need different corrections; the NPC prefabs may store their own tested values.
    /// </summary>
    [Tooltip("Manual anti-sliding correction. Higher values move the legs faster; lower values slow them down. Final MoveSpeed = agent speed / reference speed * this multiplier.")]
    [SerializeField] private float animationSpeedMultiplier = 1.5f;

    /// <summary>Animator parameter receiving raw movement speed for state transitions.</summary>
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    /// <summary>Animator parameter marking a direct NPC interaction.</summary>
    private static readonly int IsInteractingHash = Animator.StringToHash("IsInteracting");

    /// <summary>Animator parameter selecting one of the configured idle animations.</summary>
    private static readonly int RandomIdleHash = Animator.StringToHash("RandomIdle");

    /// <summary>Animator parameter receiving the corrected walk-cycle playback factor.</summary>
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

    /// <summary>
    /// Number of idle IDs expected by the Animator Controller.
    /// The current controllers use the values 0, 1 and 2, and the selection avoids repeating the current value.
    /// </summary>
    private const int IdleAnimationCount = 3;

    /// <summary>
    /// Extra distance added to the agent stopping distance before animation speed is forced to zero.
    /// The 0.15 unit margin prevents a tiny remaining path from keeping the walk animation active at the destination.
    /// </summary>
    private const float StopAnimationMargin = 0.15f;

    /// <summary>
    /// Squared minimum desired direction used before rotating the character.
    /// Ignoring smaller vectors reduces visible rotation jitter while the agent is nearly stopped.
    /// </summary>
    private const float MinimumDirectionSqrMagnitude = 0.05f;

    /// <summary>
    /// Spherical interpolation factor used for turning.
    /// The value 12 makes the character react quickly without snapping immediately to the new direction.
    /// </summary>
    private const float RotationLerpSpeed = 12f;

    /// <summary>Fills component references when this component is reset in Unity.</summary>
    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    /// <summary>Finds required components that were not assigned in the Inspector.</summary>
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Updates movement, idle selection and facing direction each frame.
    /// Speed receives the real agent speed and controls movement transitions in the Animator Controller.
    /// MoveSpeed receives the corrected playback factor used to visually match the walk cycle to that movement.
    /// </summary>
    private void Update()
    {
        if (agent == null || animator == null)
            return;

        float speed = agent.velocity.magnitude;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + StopAnimationMargin)
        {
            speed = 0f;
        }

        if (speed == 0f)
        {
            int oldValue = animator.GetInteger(RandomIdleHash);
            int newValue = Random.Range(0, IdleAnimationCount - 1);

            if (newValue >= oldValue)
            {
                newValue++;
            }

            animator.SetInteger(RandomIdleHash, newValue);
        }

        animator.SetFloat(SpeedHash, speed);

        float animationSpeed = speed / referenceWalkSpeed;
        animator.SetFloat(MoveSpeedHash, animationSpeed * animationSpeedMultiplier);

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        if (direction.sqrMagnitude > MinimumDirectionSqrMagnitude)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * RotationLerpSpeed
            );
        }

        Vector3 localPos = animator.transform.localPosition;
        localPos.x = 0f;
        localPos.z = 0f;
        animator.transform.localPosition = localPos;
    }

    /// <summary>Sets whether the interaction animation should be active.</summary>
    /// <param name="isInteracting">True while the NPC is in a direct interaction.</param>
    public void SetInteracting(bool isInteracting)
    {
        if (animator != null)
            animator.SetBool(IsInteractingHash, isInteracting);
    }
}
