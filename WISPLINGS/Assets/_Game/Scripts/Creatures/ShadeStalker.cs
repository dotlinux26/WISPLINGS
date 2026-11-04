using UnityEngine;
using GhostSurvival.Animation;
using GhostSurvival.Characters;

namespace GhostSurvival.Creatures
{
    /// <summary>
    /// AI controller for Shade Stalker enemy.
    /// Detects player within radius, tracks target, faces in 4 directions, and attacks on contact.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShadeStalker : MonoBehaviour
    {
        [Header("AI & Movement Settings")]
        [Tooltip("Detection radius to start chasing the player.")]
        [SerializeField] private float detectionRadius = 6.0f;

        [Tooltip("Minimum distance to stop before the target.")]
        [SerializeField] private float stopRadius = 0.8f;

        [Tooltip("Movement speed while chasing.")]
        [SerializeField] private float chaseSpeed = 2.2f;

        [Header("Combat Settings")]
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackCooldown = 1.2f;

        [Header("Animation & Rendering")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Hash IDs for Animator
        private static readonly int ParamDirection = Animator.StringToHash("Direction");
        private static readonly int ParamIsChasing = Animator.StringToHash("IsChasing");

        private Rigidbody2D rb;
        private Transform target;
        private IDamageable targetDamageable;
        private GhostDirection facingDirection = GhostDirection.Down;
        private bool isChasing = false;
        private float lastAttackTime = -99f;

        public bool IsChasing => isChasing;
        public GhostDirection FacingDirection => facingDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Start()
        {
            FindTarget();
        }

        private void Update()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            Vector2 diff = target.position - transform.position;
            float dist = diff.magnitude;

            if (dist <= detectionRadius)
            {
                isChasing = true;
                UpdateFacingDirection(diff);

                // Attack check
                if (dist <= stopRadius && Time.time >= lastAttackTime + attackCooldown)
                {
                    AttackTarget();
                }
            }
            else
            {
                isChasing = false;
            }

            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            if (!isChasing || target == null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector2.zero;
#else
                rb.velocity = Vector2.zero;
#endif
                return;
            }

            Vector2 diff = target.position - transform.position;
            float dist = diff.magnitude;

            if (dist > stopRadius)
            {
                Vector2 moveDir = diff.normalized;
                Vector2 targetVel = moveDir * chaseSpeed;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = targetVel;
#else
                rb.velocity = targetVel;
#endif
            }
            else
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector2.zero;
#else
                rb.velocity = Vector2.zero;
#endif
            }
        }

        private void UpdateFacingDirection(Vector2 directionToTarget)
        {
            if (Mathf.Abs(directionToTarget.x) > Mathf.Abs(directionToTarget.y))
            {
                facingDirection = directionToTarget.x > 0 ? GhostDirection.Right : GhostDirection.Left;
            }
            else
            {
                facingDirection = directionToTarget.y > 0 ? GhostDirection.Up : GhostDirection.Down;
            }
        }

        private void UpdateAnimator()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetInteger(ParamDirection, (int)facingDirection);
                animator.SetBool(ParamIsChasing, isChasing);
            }
        }

        private void AttackTarget()
        {
            lastAttackTime = Time.time;
            if (targetDamageable != null && targetDamageable.IsAlive)
            {
                targetDamageable.TakeDamage(attackDamage);
                Debug.Log($"[ShadeStalker] Attacked player for {attackDamage} damage!");
            }
        }

        private void FindTarget()
        {
            var player = FindFirstObjectByType<GhostController>();
            if (player != null)
            {
                target = player.transform;
                targetDamageable = player.GetComponent<IDamageable>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopRadius);
        }
    }
}

