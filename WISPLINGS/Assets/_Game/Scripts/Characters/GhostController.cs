using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using GhostSurvival.Animation;

namespace GhostSurvival.Characters
{
    /// <summary>
    /// Controls 2D top-down movement, physics, and facing direction for the main ghost.
    /// Normalizes diagonal movement and preserves last facing direction when idle.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class GhostController : MonoBehaviour, IDamageable
    {
        [Header("Movement Settings")]
        [Tooltip("Base movement speed in units per second.")]
        [SerializeField] private float moveSpeed = 4.0f;

        [Tooltip("Acceleration smoothing factor.")]
        [SerializeField] private float acceleration = 25.0f;

        [Header("Floating / Breathing Effect")]
        [Tooltip("Enable subtle vertical hovering without conflicting with authored walk/idle frames.")]
        [SerializeField] private bool enableSubtleHover = true;

        [SerializeField] private float hoverFrequency = 1.2f;
        [SerializeField] private float hoverAmplitude = 0.025f;
        [SerializeField] private Transform visualRoot;

        [Header("Health / Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        // References
        private Rigidbody2D rb;
        private GhostAnimator ghostAnimator;

        // Runtime state
        private Vector2 rawInput;
        private Vector2 moveInput;
        private Vector2 currentVelocity;
        private GhostDirection facingDirection = GhostDirection.Down;
        private bool isMoving = false;
        private float hoverTimer = 0f;
        private Vector3 initialVisualLocalPos = Vector3.zero;

        public bool IsAlive => currentHealth > 0f;
        public GhostDirection FacingDirection => facingDirection;
        public bool IsMoving => isMoving;
        public Vector2 MoveVelocity => currentVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ghostAnimator = GetComponent<GhostAnimator>();

            // Configure Rigidbody2D for top-down 2D
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (visualRoot == null)
            {
                visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
            }

            if (visualRoot != null && visualRoot != transform)
            {
                initialVisualLocalPos = visualRoot.localPosition;
            }
        }

        private void Update()
        {
            ReadInput();
            UpdateFacingDirection();
            UpdateHoverEffect();

            if (ghostAnimator != null)
            {
                ghostAnimator.UpdateAnimationState(facingDirection, isMoving);
            }
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = moveInput * moveSpeed;
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = currentVelocity;
#else
            rb.velocity = currentVelocity;
#endif
        }

        private void ReadInput()
        {
            rawInput = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) rawInput.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) rawInput.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) rawInput.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) rawInput.x += 1f;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (rawInput == Vector2.zero)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) rawInput.y += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) rawInput.y -= 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) rawInput.x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) rawInput.x += 1f;
            }
#endif

            // Normalize diagonal movement to prevent faster diagonal speed
            moveInput = rawInput.sqrMagnitude > 1f ? rawInput.normalized : rawInput;
            isMoving = moveInput.sqrMagnitude > 0.01f;
        }

        private void UpdateFacingDirection()
        {
            if (!isMoving)
            {
                // Khi dừng di chuyển: nếu trước đó đang đi lên (Up), quay về Idle_Down để lộ mặt cute theo yêu cầu
                if (facingDirection == GhostDirection.Up)
                {
                    facingDirection = GhostDirection.Down;
                }
                return;
            }

            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                facingDirection = moveInput.x > 0 ? GhostDirection.Right : GhostDirection.Left;
            }
            else
            {
                facingDirection = moveInput.y > 0 ? GhostDirection.Up : GhostDirection.Down;
            }
        }

        private void UpdateHoverEffect()
        {
            if (!enableSubtleHover || visualRoot == null || visualRoot == transform) return;

            hoverTimer += Time.deltaTime * hoverFrequency;
            float offset = Mathf.Sin(hoverTimer) * hoverAmplitude;
            visualRoot.localPosition = initialVisualLocalPos + new Vector3(0f, offset, 0f);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Debug.Log($"[GhostController] Took {amount} damage. Current HP: {currentHealth}/{maxHealth}");
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }
    }
}
