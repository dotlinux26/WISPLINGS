using UnityEngine;

namespace GhostSurvival.Animation
{
    /// <summary>
    /// Updates Animator states and parameters for 4-direction idle and walk.
    /// Exposes directional events so cosmetics remain frame-locked with the body.
    /// </summary>
    public class GhostAnimator : MonoBehaviour
    {
        [Header("Animator Reference")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer bodyRenderer;

        // Hash IDs for fast Animator parameter updates
        private static readonly int ParamDirection = Animator.StringToHash("Direction");
        private static readonly int ParamIsMoving = Animator.StringToHash("IsMoving");

        private GhostDirection currentDirection = GhostDirection.Down;
        private bool currentIsMoving = false;

        public GhostDirection CurrentDirection => currentDirection;
        public bool CurrentIsMoving => currentIsMoving;
        public SpriteRenderer BodyRenderer => bodyRenderer;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (bodyRenderer == null)
            {
                // Find renderer named BodyRenderer or first SpriteRenderer in children
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

        public void UpdateAnimationState(GhostDirection direction, bool isMoving)
        {
            currentDirection = direction;
            currentIsMoving = isMoving;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetInteger(ParamDirection, (int)direction);
                animator.SetBool(ParamIsMoving, isMoving);
            }
        }
    }
}
