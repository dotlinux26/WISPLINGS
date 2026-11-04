using System.Collections.Generic;
using UnityEngine;
using GhostSurvival.Cosmetics;

namespace GhostSurvival.Animation
{
    /// <summary>
    /// Updates character animation for 4-direction idle and walk.
    /// Drives sprite playback directly in C# for instant, zero-delay responsiveness,
    /// and immediately synchronizes Hat and Wings on the exact same frame.
    /// Guarantees exact directional frame mapping via dictionary and canonical 32-element array.
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

        [Header("Idle Blink Settings (Natural, occasional blinking)")]
        [Tooltip("Minimum time (seconds) between natural idle eye blinks.")]
        [SerializeField] private float minBlinkInterval = 3.5f;

        [Tooltip("Maximum time (seconds) between natural idle eye blinks.")]
        [SerializeField] private float maxBlinkInterval = 6.0f;

        [Tooltip("Duration of a single eye blink in seconds.")]
        [SerializeField] private float blinkDuration = 0.16f;

        [Header("Sprite Frames (64 frames: 8 directions x 8 frames)")]
        [SerializeField] private Sprite[] bodySprites = new Sprite[64];

        private GhostDirection currentDirection = GhostDirection.Down;
        private bool currentIsMoving = false;
        private float frameTimer = 0f;
        private int currentFrame = 0;

        // Natural Idle bobbing & blinking runtime state
        private float blinkTimer = 0f;
        private float nextBlinkInterval = 4.0f;
        private bool isBlinking = false;
        private float blinkElapsed = 0f;
        private int idleBobIndex = 0;
        private static readonly int[] idleBobSequence = new int[] { 0, 1, 2, 1 };

        private readonly Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();

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

            ResolveBodyRenderer();
            EnsureSpritesLoaded();
            BuildSpriteMap();
        }

        private void Start()
        {
            ResolveBodyRenderer();
            EnsureSpritesLoaded();
            BuildSpriteMap();
            nextBlinkInterval = Random.Range(minBlinkInterval, maxBlinkInterval);
            ApplyCurrentFrameImmediate();
        }

        public void ResolveBodyRenderer()
        {
            if (bodyRenderer == null)
            {
                var renderers = GetComponentsInChildren<SpriteRenderer>(true);
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

        private void Update()
        {
            UpdateSpritePlayback();
        }

        private void UpdateSpritePlayback()
        {
            if (bodyRenderer == null) return;

            if (currentIsMoving)
            {
                // Walking: simple continuous 4-step loop
                isBlinking = false;
                blinkTimer = 0f;
                frameTimer += Time.deltaTime;

                if (frameTimer >= 1f / walkFps)
                {
                    frameTimer -= 1f / walkFps;
                    currentFrame = (currentFrame + 1) % 4;
                    ApplyCurrentFrameImmediate();
                }
            }
            else
            {
                // Idle: smooth floating bob (0 -> 1 -> 2 -> 1) with natural, occasional blink
                // 1. Advance the smooth floating bob
                frameTimer += Time.deltaTime;
                if (frameTimer >= 1f / idleFps)
                {
                    frameTimer -= 1f / idleFps;
                    idleBobIndex = (idleBobIndex + 1) % idleBobSequence.Length;
                    if (!isBlinking)
                    {
                        currentFrame = idleBobSequence[idleBobIndex];
                        ApplyCurrentFrameImmediate();
                    }
                }

                // 2. Trigger occasional natural blink (~once every 3.5 - 6.0 seconds)
                if (!isBlinking)
                {
                    blinkTimer += Time.deltaTime;
                    if (blinkTimer >= nextBlinkInterval)
                    {
                        isBlinking = true;
                        blinkElapsed = 0f;
                        currentFrame = 3; // Cell 3 is the blink frame
                        ApplyCurrentFrameImmediate();
                    }
                }
                else
                {
                    blinkElapsed += Time.deltaTime;
                    if (blinkElapsed >= blinkDuration)
                    {
                        isBlinking = false;
                        blinkTimer = 0f;
                        nextBlinkInterval = Random.Range(minBlinkInterval, maxBlinkInterval);
                        currentFrame = idleBobSequence[idleBobIndex]; // Reopen eyes seamlessly
                        ApplyCurrentFrameImmediate();
                    }
                }
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
                idleBobIndex = 0;
                isBlinking = false;
                currentFrame = 0;
                ApplyCurrentFrameImmediate();
            }
        }

        private void ApplyCurrentFrameImmediate()
        {
            if (bodyRenderer == null) return;

            string dirStr = currentDirection.ToString();
            string stateStr = currentIsMoving ? "Walk" : "Idle";
            string targetFrameName = $"Ghost_{dirStr}_{stateStr}_{currentFrame}";

            if (!spriteMap.TryGetValue(targetFrameName, out Sprite nextSprite))
            {
                // Fallback to array index
                int dirOffset = (int)currentDirection * 8;
                int stateOffset = currentIsMoving ? 4 : 0;
                int spriteIndex = dirOffset + stateOffset + currentFrame;
                if (bodySprites != null && spriteIndex >= 0 && spriteIndex < bodySprites.Length)
                {
                    nextSprite = bodySprites[spriteIndex];
                }
            }

            if (nextSprite == null && bodySprites != null && bodySprites.Length > 0)
            {
                nextSprite = bodySprites[0];
            }

            if (nextSprite != null)
            {
                bodyRenderer.sprite = nextSprite;

                // Instantly update Hat and Wings on the exact same CPU frame
                if (cosmeticSynchronizer != null)
                {
                    cosmeticSynchronizer.SyncCosmetics(targetFrameName);
                }
            }
        }

        private void BuildSpriteMap()
        {
            // Populate dictionary from serialized bodySprites array
            if (bodySprites != null)
            {
                foreach (var s in bodySprites)
                {
                    if (s != null && s.name.StartsWith("Ghost_"))
                    {
                        spriteMap[s.name] = s;
                    }
                }
            }

#if UNITY_EDITOR
            if (spriteMap.Count < 64)
            {
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/ghost_main_8x4.png"))
                {
                    if (obj is Sprite s && s.name.StartsWith("Ghost_"))
                    {
                        spriteMap[s.name] = s;
                    }
                }
            }
#endif

            // Rebuild canonical 64-element bodySprites array
            if (bodySprites == null || bodySprites.Length != 64)
            {
                bodySprites = new Sprite[64];
            }

            string[] dirs = { "Down", "DownLeft", "Left", "UpLeft", "Up", "UpRight", "Right", "DownRight" };
            int idx = 0;
            for (int d = 0; d < 8; d++)
            {
                string dir = dirs[d];
                for (int f = 0; f < 4; f++)
                {
                    string name = $"Ghost_{dir}_Idle_{f}";
                    if (spriteMap.TryGetValue(name, out Sprite s)) bodySprites[idx] = s;
                    idx++;
                }
                for (int f = 0; f < 4; f++)
                {
                    string name = $"Ghost_{dir}_Walk_{f}";
                    if (spriteMap.TryGetValue(name, out Sprite s)) bodySprites[idx] = s;
                    idx++;
                }
            }
        }

        private void EnsureSpritesLoaded()
        {
#if UNITY_EDITOR
            if (bodySprites == null || bodySprites.Length < 64 || bodySprites[0] == null)
            {
                var dict = new Dictionary<string, Sprite>();
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/ghost_main_8x4.png"))
                {
                    if (obj is Sprite s && s.name.StartsWith("Ghost_"))
                    {
                        dict[s.name] = s;
                        spriteMap[s.name] = s;
                    }
                }

                if (dict.Count >= 64)
                {
                    bodySprites = new Sprite[64];
                    string[] dirs = { "Down", "DownLeft", "Left", "UpLeft", "Up", "UpRight", "Right", "DownRight" };
                    int idx = 0;
                    for (int d = 0; d < 8; d++)
                    {
                        string dir = dirs[d];
                        for (int f = 0; f < 4; f++)
                        {
                            dict.TryGetValue($"Ghost_{dir}_Idle_{f}", out bodySprites[idx]);
                            idx++;
                        }
                        for (int f = 0; f < 4; f++)
                        {
                            dict.TryGetValue($"Ghost_{dir}_Walk_{f}", out bodySprites[idx]);
                            idx++;
                        }
                    }
                }
            }
#endif
        }
    }
}
