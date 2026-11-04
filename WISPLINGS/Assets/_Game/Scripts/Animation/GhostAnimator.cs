using System.Collections.Generic;
using UnityEngine;
using GhostSurvival.Cosmetics;

namespace GhostSurvival.Animation
{
    /// <summary>
    /// Updates character animation for 4-direction idle and walk.
    /// Drives sprite playback directly in C# for instant, zero-delay responsiveness,
    /// and immediately synchronizes Hat, Wings, and Hands on the exact same frame.
    /// Disables Animator conflict so there is zero stutter or lag.
    /// </summary>
    public class GhostAnimator : MonoBehaviour
    {
        [Header("Renderers & Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private CosmeticSynchronizer cosmeticSynchronizer;

        [Header("Animation Speed Settings")]
        [Tooltip("Playback speed for Idle floating (calm and floaty).")]
        [SerializeField] private float idleFps = 6.0f;

        [Tooltip("Playback speed for Walk footsteps (snappy and responsive).")]
        [SerializeField] private float walkFps = 8.5f;

        [Header("Sprite Frames (32 frames: 4 directions x 8 frames)")]
        [SerializeField] private Sprite[] bodySprites = new Sprite[32];

        private GhostDirection currentDirection = GhostDirection.Down;
        private bool currentIsMoving = false;
        private float frameTimer = 0f;
        private int currentFrame = 0;

        public GhostDirection CurrentDirection => currentDirection;
        public bool CurrentIsMoving => currentIsMoving;
        public SpriteRenderer BodyRenderer => bodyRenderer;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Disable Animator component so it doesn't conflict/fight C# sprite updates
            if (animator != null)
            {
                animator.enabled = false;
            }

            if (cosmeticSynchronizer == null)
            {
                cosmeticSynchronizer = GetComponent<CosmeticSynchronizer>();
                if (cosmeticSynchronizer == null)
                {
                    cosmeticSynchronizer = GetComponentInChildren<CosmeticSynchronizer>();
                }
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
            // Immediately apply frame 0 of Idle_Down on game start
            ApplyCurrentFrameImmediate();
        }

        private void Update()
        {
            UpdateSpritePlayback();
        }

        private void UpdateSpritePlayback()
        {
            if (bodySprites == null || bodySprites.Length < 32 || bodyRenderer == null) return;

            float targetFps = currentIsMoving ? walkFps : idleFps;
            frameTimer += Time.deltaTime;

            if (frameTimer >= 1f / targetFps)
            {
                frameTimer -= 1f / targetFps;
                currentFrame = (currentFrame + 1) % 4;
                ApplyCurrentFrameImmediate();
            }
        }

        public void UpdateAnimationState(GhostDirection direction, bool isMoving)
        {
            bool stateChanged = (currentDirection != direction) || (currentIsMoving != isMoving);

            currentDirection = direction;
            currentIsMoving = isMoving;

            if (stateChanged)
            {
                // Instantly snap to frame 0 with ZERO delay on direction or movement change
                frameTimer = 0f;
                currentFrame = 0;
                ApplyCurrentFrameImmediate();
            }
        }

        private void ApplyCurrentFrameImmediate()
        {
            if (bodySprites == null || bodySprites.Length < 32 || bodyRenderer == null) return;

            int dirOffset = (int)currentDirection * 8;
            int stateOffset = currentIsMoving ? 4 : 0;
            int spriteIndex = dirOffset + stateOffset + currentFrame;

            if (spriteIndex >= 0 && spriteIndex < bodySprites.Length && bodySprites[spriteIndex] != null)
            {
                Sprite nextSprite = bodySprites[spriteIndex];
                bodyRenderer.sprite = nextSprite;

                // Instantly update Hat, Wings, and Hands on the exact same CPU frame
                if (cosmeticSynchronizer != null)
                {
                    cosmeticSynchronizer.SyncCosmetics(nextSprite.name);
                }
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
