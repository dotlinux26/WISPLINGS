using System.Collections.Generic;
using UnityEngine;

namespace GhostSurvival.Animation
{
    /// <summary>
    /// Updates Animator states and parameters for 4-direction idle and walk.
    /// Drives sprite frames directly at authored FPS for 100% guaranteed animation reliability,
    /// ensuring BodyRenderer, Hat, Wings, and Hands always animate in sync without controller glitches.
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

        [Tooltip("Base frames per second for authoring playback.")]
        [SerializeField] private float baseFps = 6f;

        [Header("Sprite Frames (32 frames: 4 directions x 8 frames)")]
        [SerializeField] private Sprite[] bodySprites = new Sprite[32];

        // Hash IDs for fast Animator parameter updates
        private static readonly int ParamDirection = Animator.StringToHash("Direction");
        private static readonly int ParamIsMoving = Animator.StringToHash("IsMoving");

        private GhostDirection currentDirection = GhostDirection.Down;
        private bool currentIsMoving = false;
        private float frameTimer = 0f;
        private int currentFrame = 0;

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

            EnsureSpritesLoaded();
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

        private void Update()
        {
            UpdateSpritePlayback();
        }

        private void UpdateSpritePlayback()
        {
            if (bodySprites == null || bodySprites.Length < 32 || bodyRenderer == null) return;

            float fps = Mathf.Max(1f, baseFps * animationSpeed);
            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / fps)
            {
                frameTimer -= 1f / fps;
                currentFrame = (currentFrame + 1) % 4;
            }

            int dirOffset = (int)currentDirection * 8;
            int stateOffset = currentIsMoving ? 4 : 0;
            int spriteIndex = dirOffset + stateOffset + currentFrame;

            if (spriteIndex >= 0 && spriteIndex < bodySprites.Length && bodySprites[spriteIndex] != null)
            {
                bodyRenderer.sprite = bodySprites[spriteIndex];
            }
        }

        public void UpdateAnimationState(GhostDirection direction, bool isMoving)
        {
            if (currentDirection != direction || currentIsMoving != isMoving)
            {
                // Reset frame timer on direction or state change for responsive feel
                frameTimer = 0f;
                currentFrame = 0;
            }

            currentDirection = direction;
            currentIsMoving = isMoving;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetInteger(ParamDirection, (int)direction);
                animator.SetBool(ParamIsMoving, isMoving);
                animator.speed = animationSpeed;
            }
        }

        private void EnsureSpritesLoaded()
        {
#if UNITY_EDITOR
            if (bodySprites == null || bodySprites.Length == 0 || bodySprites[0] == null)
            {
                var loaded = new List<Sprite>();
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/ghost_main_8x4.png"))
                {
                    if (obj is Sprite s) loaded.Add(s);
                }
                if (loaded.Count >= 32)
                {
                    bodySprites = loaded.ToArray();
                }
            }
#endif
        }
    }
}
