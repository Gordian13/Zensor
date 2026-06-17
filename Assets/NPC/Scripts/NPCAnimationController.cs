using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    

    [SerializeField] private float referenceWalkSpeed = 2f;
    [SerializeField] private float animationSpeedMultiplier = 1.5f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int IsInteractingHash = Animator.StringToHash("IsInteracting");

    private static readonly int RandomIdleHash = Animator.StringToHash("RandomIdle");

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (agent == null || animator == null)
            return;

        float speed = agent.velocity.magnitude;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.15f)
            speed = 0f;

        if (speed == 0f)
        {
            animator.SetInteger(
                RandomIdleHash,
                // Set range up for more animations
                Random.Range(0, 2)
            );
        }
        animator.SetFloat(SpeedHash, speed);

        // Animation Speed
        float animationSpeed = speed / referenceWalkSpeed;
        animator.SetFloat("MoveSpeed", animationSpeed * animationSpeedMultiplier);
        // Make sure the NPC faces the direction it's moving in
        // TODO: This can cause jittery rotation when the NPC is stopping, consider smoothing it out or only rotating when speed is above a certain threshold
        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 12f
            );
        }
        Vector3 localPos = animator.transform.localPosition;
        localPos.x = 0f;
        localPos.z = 0f;
        animator.transform.localPosition = localPos;
    }

    public void SetInteracting(bool isInteracting)
    {
        if (animator != null)
            animator.SetBool(IsInteractingHash, isInteracting);
    }
}