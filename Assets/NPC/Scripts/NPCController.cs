using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls one NPC while the game is running.
/// It connects profile data, moods, reactions, patrol movement, dialogue and animation.
/// </summary>
/// <remarks>
/// Values saved in a scene or prefab override the initial values shown in this script.
/// The initial values are safe starting points for a newly added component, while individual NPC prefabs can keep their own tuned setup.
/// </remarks>
public class NPCController : MonoBehaviour
{
    /// <summary>
    /// Delay before checking again when patrol has no configured points.
    /// Waiting one second prevents a warning and coroutine iteration on every frame while still noticing points added at runtime.
    /// </summary>
    private const float MissingPatrolPointRetryDelay = 1f;

    /// <summary>Fallback wait time when the current mood has no matching NPCMoodData entry.</summary>
    private const float MissingMoodWaitTime = 1f;

    /// <summary>
    /// Maximum attempts made while looking for a random point different from the previous target.
    /// The limit prevents a damaged list or unusual setup from keeping the coroutine in an endless selection loop.
    /// </summary>
    private const int RandomPointSelectionAttempts = 25;

    /// <summary>
    /// Squared horizontal distance below which facing a target is skipped.
    /// This avoids creating a rotation from an almost zero-length direction.
    /// </summary>
    private const float MinimumFacingDirectionSqrMagnitude = 0.001f;

    /// <summary>
    /// Small velocity assigned just before the patrol path is reset.
    /// It keeps the original hard-stop behavior that was used to reduce visible NavMesh braking or sliding.
    /// </summary>
    private static readonly Vector3 PatrolStopVelocity = new Vector3(0.01f, 0f, 0.01f);

    /// <summary>Radius of each regular patrol-point sphere in the Scene view.</summary>
    private const float PatrolPointGizmoRadius = 0.25f;

    /// <summary>
    /// Radius of the current-target sphere in the Scene view.
    /// It is larger than a regular patrol point so the active destination is easy to recognize.
    /// </summary>
    private const float CurrentTargetGizmoRadius = 0.4f;

    /// <summary>
    /// Reusable data asset containing the NPC's name, mood settings, reaction rules, interactions and default dialogue.
    /// Assigning a different profile changes content and behavior without requiring another controller script.
    /// </summary>
    [Header("NPC Profile")]
    [Tooltip("Reusable NPC data asset containing the name, mood settings, reactions, menu interactions and default dialogue.")]
    [SerializeField] private NPCProfile profile;

    /// <summary>
    /// Mood active when the NPC is loaded.
    /// During <see cref="Awake"/>, the matching profile entry supplies the agent speed and patrol stopping distance.
    /// Reaction rules can replace this value later by calling <see cref="SetMood"/>.
    /// </summary>
    [Header("Mood State Machine")]
    [Tooltip("Mood used when the NPC starts. Its matching profile entry controls movement speed and patrol waiting time.")]
    [SerializeField] private NPCMood currentMood = NPCMood.Normal;

    /// <summary>
    /// NavMeshAgent responsible for path finding and movement.
    /// The controller changes its speed from mood data and temporarily changes its stopping distance while moving to an interaction point.
    /// Automatic agent rotation is disabled because <see cref="NPCAnimationController"/> turns the visible character toward its movement direction.
    /// </summary>
    [Header("Movement")]
    [Tooltip("NavMeshAgent used for patrol and interaction movement. Usually the agent on this GameObject.")]
    [SerializeField] private NavMeshAgent agent;

    /// <summary>
    /// Distance from the interaction anchor at which the NPC is considered to have arrived.
    /// The default 1.5 units leaves space between the NPC and the player instead of moving both objects onto the same position.
    /// This temporarily replaces the agent's normal stopping distance and is restored after arrival or cancellation.
    /// </summary>
    [Tooltip("Distance kept from the interaction anchor. 1.5 leaves a natural gap between the NPC and player.")]
    [SerializeField] private float interactionArrivalDistance = 1.5f;

    /// <summary>
    /// Scene transforms the NPC can visit while patrolling.
    /// With random selection they form a pool; with sequential selection their list order becomes the walking order.
    /// Null entries are not useful and can cause the selected target to be skipped.
    /// </summary>
    [Tooltip("Scene points used by the patrol. Their list order matters when random selection is disabled.")]
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

    /// <summary>
    /// Distance from a patrol point at which the agent counts the point as reached.
    /// The default 0.5 avoids requiring an exact position, which can be difficult on a baked NavMesh.
    /// This value is copied to the agent when mood settings are applied; interaction movement uses its own arrival distance.
    /// </summary>
    [Tooltip("Arrival distance for patrol points. 0.5 avoids waiting for an exact position on the NavMesh.")]
    [SerializeField] private float stoppingDistance = 0.5f;

    /// <summary>
    /// Whether the patrol coroutine starts automatically after the optional welcome interaction.
    /// Disable it for stationary NPCs or when another script should decide when patrol begins.
    /// </summary>
    [Header("Patrol Behaviour")]
    [Tooltip("Starts patrolling automatically after the optional welcome dialogue. Disable for stationary or externally controlled NPCs.")]
    [SerializeField] private bool startPatrollingOnStart = true;

    /// <summary>
    /// Chooses how the next patrol point is selected.
    /// When enabled a random list entry is used; when disabled the list is visited from top to bottom and then repeated.
    /// </summary>
    [Tooltip("Enabled: choose random patrol points. Disabled: visit them in list order.")]
    [SerializeField] private bool pickRandomPoints = true;

    /// <summary>
    /// Prevents random patrol from immediately choosing the previous point again.
    /// This setting has no effect in sequential mode and is unnecessary when only one patrol point exists.
    /// </summary>
    [Tooltip("In random mode, prevents selecting the same patrol point twice in a row.")]
    [SerializeField] private bool avoidSamePointTwice = true;

    /// <summary>
    /// Whether clicks and scripted interaction requests are allowed to start a dialogue with this NPC.
    /// Movement, animation and ambient behavior can still run while this option is disabled.
    /// </summary>
    [Header("Interactions")]
    [Tooltip("Allows this NPC to start direct interactions. Disable for decorative or temporarily unavailable NPCs.")]
    [SerializeField] private bool isInteractable = true;

    /// <summary>Read-only access to the Inspector setting that allows direct interaction.</summary>
    public bool IsInteractable => isInteractable;

    /// <summary>
    /// Whether the NPC automatically starts a special dialogue shortly after appearing.
    /// The normal patrol waits until this interaction has finished.
    /// </summary>
    [Tooltip("Starts the welcome dialogue automatically after the NPC appears, before automatic patrol begins.")]
    [SerializeField] private bool doWelcomeInteractionAfterSpawn;

    /// <summary>
    /// Dialogue used only by the automatic welcome interaction.
    /// It is separate from <see cref="NPCProfile.defaultDialogueScript"/>, which is used for normal player-started conversations.
    /// </summary>
    [Tooltip("Dialogue used by the automatic welcome interaction. Normal clicks use the default dialogue from the NPC profile.")]
    [SerializeField] private NPCDialogueScript welcomeDialogueScript;

    /// <summary>
    /// Time in seconds before an enabled welcome interaction starts.
    /// The small 0.1 second default lets other scene objects finish their Awake setup before the interaction handler is requested.
    /// </summary>
    [Tooltip("Delay before the automatic welcome dialogue. A short delay gives scene managers time to initialize.")]
    [SerializeField] private float welcomeInteractionDelay = 0.1f;

    /// <summary>
    /// Additional distance the NPC may lose after reaching its closest point to a moving interaction anchor.
    /// If the current distance becomes larger than the shortest measured distance plus this tolerance, the walk is cancelled.
    /// The default 1 unit allows small movement without letting the NPC chase a target that is clearly moving away.
    /// </summary>
    [Tooltip("Cancels interaction movement when the anchor moves this far away after the NPC had already moved closer.")]
    [SerializeField] private float interactionMoveAwayTolerance = 1f;

    /// <summary>
    /// Time in seconds between path updates while following the interaction anchor.
    /// The default 0.15 seconds reacts quickly to small anchor movement without recalculating the destination every frame.
    /// Lower values follow moving anchors more closely but request new NavMesh paths more often.
    /// </summary>
    [Tooltip("Seconds between destination updates while approaching the interaction anchor. Lower is more responsive but recalculates paths more often.")]
    [SerializeField] private float interactionDestinationRefreshRate = 0.15f;

    /// <summary>
    /// Animation bridge for movement and interaction parameters.
    /// The controller uses it to set IsInteracting while patrol movement is paused.
    /// </summary>
    [Header("Animation")]
    [Tooltip("Component that updates the NPC Animator and receives the interaction state.")]
    [SerializeField] private NPCAnimationController animationController;

    /// <summary>
    /// Whether patrol points, connecting lines and the active target are drawn in Unity's Scene view.
    /// This only affects editor helpers and has no effect on the built game's visuals.
    /// </summary>
    [Header("Debug")]
    [Tooltip("Draws patrol helpers in the Scene view. This does not affect the built game.")]
    [SerializeField] private bool drawGizmos = true;

    /// <summary>
    /// Patrol target currently selected by the running patrol loop.
    /// It is serialized only so it can be inspected while debugging; normally it should not be assigned manually.
    /// </summary>
    [Tooltip("Runtime debug value showing the active patrol target. Normally do not assign this manually.")]
    [SerializeField] private Transform currentTarget;

    /// <summary>The reusable profile assigned to this NPC.</summary>
    public NPCProfile Profile => profile;

    /// <summary>The readable profile name or a fallback when no profile exists.</summary>
    public string NPCName => profile != null ? profile.npcName : "Unnamed NPC";

    /// <summary>The NPC's active mood.</summary>
    public NPCMood CurrentMood => currentMood;

    /// <summary>The points used by the patrol loop.</summary>
    public List<Transform> PatrolPoints => patrolPoints;

    /// <summary>Whether the NPC is currently part of a direct interaction.</summary>
    public bool IsInteracting => isInteracting;

    /// <summary>Currently running patrol loop, or null while patrol is stopped.</summary>
    private Coroutine patrolRoutine;

    /// <summary>
    /// Index used by sequential patrol selection.
    /// It starts at -1 so the first increment selects list index 0.
    /// </summary>
    private int currentPatrolIndex = -1;

    /// <summary>Previous random patrol target, used by the avoid-same-point option.</summary>
    private Transform lastTarget;

    /// <summary>Runtime flag that pauses ambient behavior and prevents starting the same interaction twice.</summary>
    private bool isInteracting;

    /// <summary>Remembers whether patrol should restart after the current interaction ends.</summary>
    private bool wasPatrollingBeforeInteraction;

    /// <summary>Currently running movement toward the interaction anchor.</summary>
    private Coroutine interactionMoveRoutine;

    /// <summary>Fills the NavMeshAgent reference when the component is reset in Unity.</summary>
    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>Finds required components and applies the starting mood.</summary>
    private void Awake()
    {
        if (animationController == null)
            animationController = GetComponent<NPCAnimationController>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        ApplyCurrentMoodSettings();
    }

    /// <summary>Runs the optional welcome dialogue before starting the patrol.</summary>
    /// <returns>An enumerator used by Unity as a coroutine.</returns>
    private IEnumerator Start()
    {
        if (doWelcomeInteractionAfterSpawn)
        {
            yield return new WaitForSeconds(welcomeInteractionDelay);

            if (NPCInteractionHandler.Instance != null)
            {
                NPCInteractionHandler.Instance.StartInteraction(
                    this,
                    welcomeDialogueScript
                );

                yield return new WaitUntil(() => !isInteracting);
            }
        }

        if (startPatrollingOnStart)
        {
            StartPatrol();
        }
    }

    /// <summary>Keeps movement settings updated after Inspector changes.</summary>
    private void OnValidate()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        ApplyCurrentMoodSettings();
    }

    /// <summary>Changes the current mood and applies its movement settings.</summary>
    /// <param name="newMood">The mood that should become active.</param>
    public void SetMood(NPCMood newMood)
    {
        if (currentMood == newMood)
            return;

        currentMood = newMood;
        ApplyCurrentMoodSettings();

        Debug.Log($"{NPCName} mood changed to {currentMood}");
    }

    /// <summary>Finds and runs the first reaction rule matching an external event.</summary>
    /// <param name="context">Information about the event from another system.</param>
    /// <returns>The NPC's reaction and whether the original action should stop.</returns>
    public NPCReactionResult ReactTo(NPCReactionContext context)
    {
        if (profile == null)
        {
            Debug.LogWarning($"{name} has no NPCProfile assigned.");
            return NPCReactionResult.NoReaction();
        }

        foreach (NPCReactionRule rule in profile.reactionRules)
        {
            if (!rule.Matches(context))
                continue;

            if (rule.changeMood)
                SetMood(rule.resultingMood);

            if (!string.IsNullOrWhiteSpace(rule.reactionText))
                Say(rule.reactionText);

            Debug.Log($"{NPCName} reacted to {context.eventType} from object '{context.objectId}'");

            return new NPCReactionResult(
                true,
                rule.blockOriginalAction,
                rule.reactionText
            );
        }

        return NPCReactionResult.NoReaction();
    }

    /// <summary>Shows a short NPC line in the log and dialogue window.</summary>
    /// <param name="text">The line the NPC should say.</param>
    public void Say(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        Debug.Log($"{NPCName} says: {text}");

        if (NPCDialogueWindow.Instance != null)
            NPCDialogueWindow.Instance.ShowReactionDialogue(text, this);
        else
            Debug.LogWarning("NPCDialogueWindow.Instance is null.");
    }

    /// <summary>Copies the active mood's movement values to the NavMeshAgent.</summary>
    private void ApplyCurrentMoodSettings()
    {
        if (agent == null)
            return;

        NPCMoodData moodData = GetCurrentMoodData();

        if (moodData == null)
            return;

        agent.speed = moodData.moveSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    /// <summary>Finds the profile data for the active mood.</summary>
    /// <returns>The matching data, or null when it is not configured.</returns>
    private NPCMoodData GetCurrentMoodData()
    {
        if (profile == null || profile.moods == null)
            return null;

        foreach (NPCMoodData moodData in profile.moods)
        {
            if (moodData.mood == currentMood)
                return moodData;
        }

        return null;
    }

    /// <summary>Starts or restarts the NPC patrol.</summary>
    public void StartPatrol()
    {
        if (patrolRoutine != null)
            StopCoroutine(patrolRoutine);

        patrolRoutine = StartCoroutine(PatrolLoop());
    }

    /// <summary>Stops the active patrol coroutine.</summary>
    public void StopPatrol()
    {
        if (patrolRoutine != null)
            StopCoroutine(patrolRoutine);

        patrolRoutine = null;

    }

    /// <summary>
    /// Moves between patrol points and waits at each point.
    /// After arrival the agent is stopped, given a tiny non-zero velocity and has its path reset to reduce visible brake sliding.
    /// Waiting time is read again from the active mood, so a mood change can affect the next pause without restarting patrol.
    /// </summary>
    /// <returns>An enumerator used by Unity as a coroutine.</returns>
    private IEnumerator PatrolLoop()
    {
        while (true)
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                Debug.LogWarning($"{name} has no patrol points assigned.");
                yield return new WaitForSeconds(MissingPatrolPointRetryDelay);
                continue;
            }

            Transform target = GetNextPatrolPoint();

            if (target == null)
            {
                yield return null;
                continue;
            }

            currentTarget = target;
            lastTarget = target;

            agent.SetDestination(target.position);

            while (agent.pathPending)
                yield return null;

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                yield return null;

            agent.isStopped = true;
            agent.velocity = PatrolStopVelocity;
            agent.ResetPath();

            NPCMoodData moodData = GetCurrentMoodData();
            float waitTime = moodData != null
                ? moodData.waitAtPointSeconds
                : MissingMoodWaitTime;

            yield return new WaitForSeconds(waitTime);

            agent.isStopped = false;
        }
    }

    /// <summary>Selects a patrol point with the configured selection mode.</summary>
    /// <returns>The next patrol point.</returns>
    private Transform GetNextPatrolPoint()
    {
        if (pickRandomPoints)
            return GetRandomPatrolPoint();

        return GetSequentialPatrolPoint();
    }

    /// <summary>Selects a random patrol point and can avoid the previous point.</summary>
    /// <returns>The selected point, or null if no suitable point was found.</returns>
    private Transform GetRandomPatrolPoint()
    {
        if (patrolPoints.Count == 1)
            return patrolPoints[0];

        Transform selectedPoint = null;
        int safetyCounter = 0;

        while (selectedPoint == null && safetyCounter < RandomPointSelectionAttempts)
        {
            Transform candidate = patrolPoints[Random.Range(0, patrolPoints.Count)];

            if (!avoidSamePointTwice || candidate != lastTarget)
                selectedPoint = candidate;

            safetyCounter++;
        }

        return selectedPoint;
    }

    /// <summary>Selects the next patrol point in list order.</summary>
    /// <returns>The selected patrol point.</returns>
    private Transform GetSequentialPatrolPoint()
    {
        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Count)
            currentPatrolIndex = 0;

        return patrolPoints[currentPatrolIndex];
    }

    /// <summary>Draws patrol points and the current target in the Scene view.</summary>
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || patrolPoints == null)
            return;

        NPCMoodData moodData = GetCurrentMoodData();

        Gizmos.color = moodData != null ? moodData.gizmoColor : Color.yellow;

        foreach (Transform point in patrolPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawSphere(point.position, PatrolPointGizmoRadius);
            Gizmos.DrawLine(transform.position, point.position);
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(currentTarget.position, CurrentTargetGizmoRadius);
        }
    }

    /// <summary>Pauses patrol movement and starts the interaction animation.</summary>
    public void BeginInteraction()
    {
        if (isInteracting)
            return;

        isInteracting = true;
        animationController?.SetInteracting(true);
        wasPatrollingBeforeInteraction = patrolRoutine != null;

        StopPatrol();
    }

    /// <summary>
    /// Ends the NPC interaction, restores patrol movement and releases the shared input block.
    /// </summary>
    public void EndInteraction()
    {
        if (!isInteracting)
            return;

        isInteracting = false;
        animationController?.SetInteracting(false);

        if (agent != null)
            agent.isStopped = false;
        
        if (interactionMoveRoutine != null)
        {
            StopCoroutine(interactionMoveRoutine);
            interactionMoveRoutine = null;
        }

        if (wasPatrollingBeforeInteraction)
            StartPatrol();

        if (GlobalInteractionState.Instance != null)
            GlobalInteractionState.Instance.UnblockInteractions();
    }

    /// <summary>Moves the NPC to the shared interaction point.</summary>
    /// <param name="anchor">The point the NPC should move to.</param>
    /// <param name="lookAtTarget">An optional object the NPC should face after arriving.</param>
    /// <param name="onArrived">The action to run after the NPC arrives.</param>
    public void MoveToInteractionAnchor(
        Transform anchor,
        Transform lookAtTarget,
        System.Action onArrived)
    {
        if (anchor == null)
        {
            Debug.LogWarning("Interaction anchor is null.");
            return;
        }

        BeginInteraction();

        if (interactionMoveRoutine != null)
            StopCoroutine(interactionMoveRoutine);

        interactionMoveRoutine = StartCoroutine(
            MoveToInteractionAnchorRoutine(anchor, lookAtTarget, onArrived)
        );
    }

    /// <summary>Turns the NPC toward a world position without tilting it.</summary>
    /// <param name="lookAtPosition">The position the NPC should face.</param>
    private void FacePosition(Vector3 lookAtPosition)
    {
        Vector3 direction = lookAtPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= MinimumFacingDirectionSqrMagnitude)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    /// <summary>Cancels movement to the interaction point and ends the interaction.</summary>
    private void CancelInteractionMove()
    {
        Debug.Log("NPC interaction cancelled because interaction anchor moved.");

        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        EndInteraction();
    }

    /// <summary>Moves an anchor position to the NPC's current height.</summary>
    /// <param name="anchor">The original interaction point.</param>
    /// <returns>The flattened world position.</returns>
    private Vector3 GetFlatAnchorPosition(Transform anchor)
    {
        Vector3 position = anchor.position;
        position.y = transform.position.y;
        return position;
    }

    /// <summary>Follows the interaction point, checks arrival and handles cancellation.</summary>
    /// <param name="anchor">The interaction point to follow.</param>
    /// <param name="lookAtTarget">The optional target to face after arriving.</param>
    /// <param name="onArrived">The action to run after arrival.</param>
    /// <returns>An enumerator used by Unity as a coroutine.</returns>
    private IEnumerator MoveToInteractionAnchorRoutine(
        Transform anchor,
        Transform lookAtTarget,
        System.Action onArrived)
    {
        if (agent == null || anchor == null)
            yield break;

        float originalStoppingDistance = agent.stoppingDistance;
        agent.stoppingDistance = interactionArrivalDistance;

        float shortestDistanceToAnchor = Vector3.Distance(transform.position, anchor.position);

        agent.isStopped = false;
        agent.SetDestination(GetFlatAnchorPosition(anchor));

        float refreshTimer = 0f;

        while (agent.pathPending)
            yield return null;

        while (true)
        {
            if (anchor == null)
            {
                agent.stoppingDistance = originalStoppingDistance;
                CancelInteractionMove();
                yield break;
            }

            float currentDistanceToAnchor = Vector3.Distance(transform.position, anchor.position);

            if (currentDistanceToAnchor < shortestDistanceToAnchor)
            {
                shortestDistanceToAnchor = currentDistanceToAnchor;
                agent.SetDestination(GetFlatAnchorPosition(anchor));
            }
            bool movedAwayAfterGettingCloser =
                currentDistanceToAnchor > shortestDistanceToAnchor + interactionMoveAwayTolerance;

            if (movedAwayAfterGettingCloser)
            {
                agent.stoppingDistance = originalStoppingDistance;
                CancelInteractionMove();
                yield break;
            }

            bool arrived =
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance;

            if (arrived)
                break;

            refreshTimer += Time.deltaTime;

            if (refreshTimer >= interactionDestinationRefreshRate)
            {
                agent.SetDestination(GetFlatAnchorPosition(anchor));
                refreshTimer = 0f;
            }

            yield return null;
        }

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.stoppingDistance = originalStoppingDistance;

        Vector3 lookPosition = lookAtTarget != null
            ? lookAtTarget.position
            : anchor.position;

        FacePosition(lookPosition);
        onArrived?.Invoke();
    }
}
