using UnityEngine;

namespace GhostSurvival.Animation
{
    /// <summary>
    /// Updates Animator states and parameters for 4-direction idle and walk.
    /// Exposes animation speed so floating/walking pace can be tuned to be soft and relaxed.
    /// </summary>
    public class GhostAnimator : MonoBehaviour
    {
        [Header("Animator Reference")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer bodyRenderer;

        [Header("Animation Pace Settings")]
        [Tooltip("Pace multiplier for the ghost animations. Lower values create a calmer, softer floating rhythm.")]
        [Range(0.2f, 2.0f)]
        [SerializeField] private float animationSpeed = 0.65f;

        // Hash IDs for fast Animator parameter updates
        private static readonly int ParamDirection = Animator.StringToHash("Direction");
        private static readonly int ParamIsMoving = Animator.StringToHash("IsMoving");

        private GhostDirection currentDirection = GhostDirection.Down;
        private bool currentIsMoving = false;

        public GhostDirection CurrentDirection => currentDirection;
        public bool CurrentIsMoving => currentIsMoving;
        public SpriteRenderer BodyRenderer => bodyRenderer;
        public float AnimationSpeed
        {
            get => animationSpeed;
            set
            {
                animationSpeed = value;
                if (animator != null) animator.speed = animationSpeed;
            }
        }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (bodyRenderer == null)
            {
                var renderers = GetComponentsInChildren<SpriteRenderer>();
                foreach (var r in renderers)
                {
                    if (r.gameObject.name.Contains("Body"))
                    {
                        bodyRenderer = r;
                        break;
                    }
                }
                if (bodyRenderer == null && renderers.Length > 0)
                {
                    bodyRenderer = renderers[0];
                }
            }
        }

        private void Start()
        {
            ApplyAnimationSpeed();
        }

        private void OnValidate()
        {
            ApplyAnimationSpeed();
        }

        public void ApplyAnimationSpeed()
        {
            if (animator != null)
            {
                animator.speed = animationSpeed;
            }
        }

        public void UpdateAnimationState(GhostDirection direction, bool isMoving)
        {
            currentDirection = direction;
            currentIsMoving = isMoving;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetInteger(ParamDirection, (int)direction);
                animator.SetBool(ParamIsMoving, isMoving);
                animator.speed = animationSpeed;
            }
        }
    }
}
