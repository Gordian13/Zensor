using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Main runtime controller for one NPC.
// Handles profile data, mood changes, patrol movement, reactions, and dialogue output.
public class NPCController : MonoBehaviour
{
    [Header("NPC Profile")]
    // ScriptableObject containing reusable NPC data: name, moods, reactions, interactions, etc.
    [SerializeField] private NPCProfile profile;

    [Header("Mood State Machine")]
    // Current mood/state of the NPC.
    [SerializeField] private NPCMood currentMood = NPCMood.Normal;

    [Header("Movement")]
    // NavMeshAgent used for pathfinding and movement.
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private float interactionArrivalDistance = 1.5f;

    // Scene patrol points the NPC can walk between.
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

    // Distance at which the NPC counts as having reached a patrol point.
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Patrol Behaviour")]
    // If true, patrol starts automatically when the scene starts.
    [SerializeField] private bool startPatrollingOnStart = true;

    // If true, the next patrol point is chosen randomly.
    // If false, patrol points are visited in list order.
    [SerializeField] private bool pickRandomPoints = true;

    // Prevents the NPC from selecting the same patrol point twice in a row.
    [SerializeField] private bool avoidSamePointTwice = true;

    [Header("Debug")]
    // Enables/disables scene gizmo drawing for patrol points.
    [SerializeField] private bool drawGizmos = true;

    // Shows the currently selected patrol target in the Inspector.
    [SerializeField] private Transform currentTarget;

    [SerializeField] private float interactionMoveAwayTolerance = 1f;
    [SerializeField] private float interactionDestinationRefreshRate = 0.15f;

    [SerializeField] private NPCAnimationController animationController;

    // Public read-only access to profile and runtime state.
    public NPCProfile Profile => profile;
    public string NPCName => profile != null ? profile.npcName : "Unnamed NPC";
    public NPCMood CurrentMood => currentMood;
    public List<Transform> PatrolPoints => patrolPoints;
    public bool IsInteracting => isInteracting;

    private Coroutine patrolRoutine;
    private int currentPatrolIndex = -1;
    private Transform lastTarget;
    private bool isInteracting;
    private bool wasPatrollingBeforeInteraction;
    private Coroutine interactionMoveRoutine;

    // Called by Unity when the component is added or reset.
    // Auto-fills the NavMeshAgent reference if possible.
    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Runtime initialization.
    private void Awake()
    {
        if (animationController == null)
            animationController = GetComponent<NPCAnimationController>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false; // We handle rotation manually in the animation controller for better control.
        if (animationController == null)
            animationController = GetComponent<NPCAnimationController>();
        ApplyCurrentMoodSettings();
    }

    // Starts automatic patrol if enabled.
    private void Start()
    {
        if (startPatrollingOnStart)
            StartPatrol();
    }

    // Runs in the editor when Inspector values change.
    // Keeps agent settings synced while editing.
    private void OnValidate()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        ApplyCurrentMoodSettings();
    }

    // Changes the NPC mood/state and applies the matching mood settings.
    public void SetMood(NPCMood newMood)
    {
        if (currentMood == newMood)
            return;

        currentMood = newMood;
        ApplyCurrentMoodSettings();

        Debug.Log($"{NPCName} mood changed to {currentMood}");
    }

    // Main reaction entry point for external systems.
    // Example: another system can send "RecordPlayed" with an objectId.
    public NPCReactionResult ReactTo(NPCReactionContext context)
    {
        if (profile == null)
        {
            Debug.LogWarning($"{name} has no NPCProfile assigned.");
            return NPCReactionResult.NoReaction();
        }

        // Check every configured reaction rule in the profile.
        foreach (NPCReactionRule rule in profile.reactionRules)
        {
            if (!rule.Matches(context))
                continue;

            // Optional mood change.
            if (rule.changeMood)
                SetMood(rule.resultingMood);

            // Optional dialogue output.
            if (!string.IsNullOrWhiteSpace(rule.reactionText))
                Say(rule.reactionText);

            Debug.Log($"{NPCName} reacted to {context.eventType} from object '{context.objectId}'");

            // Return whether the NPC reacted and whether the external action should be blocked.
            return new NPCReactionResult(
                true,
                rule.blockOriginalAction,
                rule.reactionText
            );
        }

        // No matching rule found.
        return NPCReactionResult.NoReaction();
    }

    // Displays NPC speech through debug log and, if available, the dialogue UI.
    public void Say(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        Debug.Log($"{NPCName} says: {text}");

        if (NPCDialogueWindow.Instance != null)
            NPCDialogueWindow.Instance.ShowReactionDialogue(text);
        else
            Debug.LogWarning("NPCDialogueWindow.Instance is null.");
    }

    // Applies movement values from the current mood data.
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

    // Finds the mood data in the profile matching the current mood.
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

    // Starts the patrol coroutine.
    public void StartPatrol()
    {
        if (patrolRoutine != null)
            StopCoroutine(patrolRoutine);

        patrolRoutine = StartCoroutine(PatrolLoop());
    }

    // Stops the patrol coroutine and clears the current NavMesh path.
    public void StopPatrol()
    {
        if (patrolRoutine != null)
            StopCoroutine(patrolRoutine);

        patrolRoutine = null;

        if (agent != null)
            agent.ResetPath();
    }

    // Main patrol loop.
    // Repeatedly selects a point, walks to it, waits, then selects the next one.
    private IEnumerator PatrolLoop()
    {
        while (true)
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                Debug.LogWarning($"{name} has no patrol points assigned.");
                yield return new WaitForSeconds(1f);
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

            // Wait until the path has been calculated.
            while (agent.pathPending)
                yield return null;

            // Wait until the NPC reaches the target.
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                yield return null;

            // Stop agent hard, to prevent brake sliding
            agent.isStopped = true;
            agent.velocity = new Vector3(0.01f, 0f, 0.01f);
            agent.ResetPath();

            // Wait time depends on current mood
            NPCMoodData moodData = GetCurrentMoodData();
            float waitTime = moodData != null ? moodData.waitAtPointSeconds : 1f;

            yield return new WaitForSeconds(waitTime);

            // Activate agent for the next patrol point
            agent.isStopped = false;
        }
    }

    // Decides whether to use random or sequential patrol behavior.
    private Transform GetNextPatrolPoint()
    {
        if (pickRandomPoints)
            return GetRandomPatrolPoint();

        return GetSequentialPatrolPoint();
    }

    // Selects a random patrol point.
    private Transform GetRandomPatrolPoint()
    {
        if (patrolPoints.Count == 1)
            return patrolPoints[0];

        Transform selectedPoint = null;
        int safetyCounter = 0;

        // Safety counter prevents an infinite loop if something goes wrong.
        while (selectedPoint == null && safetyCounter < 25)
        {
            Transform candidate = patrolPoints[Random.Range(0, patrolPoints.Count)];

            if (!avoidSamePointTwice || candidate != lastTarget)
                selectedPoint = candidate;

            safetyCounter++;
        }

        return selectedPoint;
    }

    // Selects patrol points in list order.
    private Transform GetSequentialPatrolPoint()
    {
        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Count)
            currentPatrolIndex = 0;

        return patrolPoints[currentPatrolIndex];
    }

    // Draws debug visuals in the Scene view when the NPC is selected.
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || patrolPoints == null)
            return;

        NPCMoodData moodData = GetCurrentMoodData();

        // Patrol gizmo color depends on current mood.
        Gizmos.color = moodData != null ? moodData.gizmoColor : Color.yellow;

        foreach (Transform point in patrolPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawSphere(point.position, 0.25f);
            Gizmos.DrawLine(transform.position, point.position);
        }

        // Current target is drawn as a white sphere.
        if (currentTarget != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(currentTarget.position, 0.4f);
        }
    }

    public void BeginInteraction()
    {
        if (isInteracting)
            return;

        isInteracting = true;
        animationController?.SetInteracting(true);
        wasPatrollingBeforeInteraction = patrolRoutine != null;

        

        StopPatrol();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

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
    }
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

    private void FacePosition(Vector3 lookAtPosition)
    {
        Vector3 direction = lookAtPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

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
        agent.SetDestination(anchor.position);

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
                agent.SetDestination(anchor.position);
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
                agent.SetDestination(anchor.position);
                refreshTimer = 0f;
            }

            yield return null;
        }

        agent.isStopped = true;
        agent.ResetPath();

        agent.stoppingDistance = originalStoppingDistance;

        Vector3 lookPosition = lookAtTarget != null
            ? lookAtTarget.position
            : anchor.position;

        FacePosition(lookPosition);

        onArrived?.Invoke();
    }
}