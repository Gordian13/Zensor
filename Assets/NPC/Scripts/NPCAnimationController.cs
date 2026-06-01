using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

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

        animator.SetFloat(SpeedHash, speed);

        // Make sure the NPC faces the direction it's moving in
        // TODO: This can cause jittery rotation when the NPC is stopping, consider smoothing it out or only rotating when speed is above a certain threshold
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(agent.velocity.normalized),
                Time.deltaTime * 8f
            );
        }
    }
}