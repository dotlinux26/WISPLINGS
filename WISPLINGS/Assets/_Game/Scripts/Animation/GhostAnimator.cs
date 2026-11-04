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

        [Header("Sprite Frames (32 frames: 4 directions x 8 frames)")]
        [SerializeField] private Sprite[] bodySprites = new Sprite[32];

        private GhostDirection currentDirection = GhostDirection.Down;
        private bool currentIsMoving = false;
        private float frameTimer = 0f;
        private int currentFrame = 0;

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
            if (spriteMap.Count < 32)
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

            // Rebuild canonical 32-element bodySprites array
            if (bodySprites == null || bodySprites.Length != 32)
            {
                bodySprites = new Sprite[32];
            }

            string[] dirs = { "Down", "Left", "Right", "Up" };
            int idx = 0;
            for (int d = 0; d < 4; d++)
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
            if (bodySprites == null || bodySprites.Length < 32 || bodySprites[0] == null)
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

                if (dict.Count >= 32)
                {
                    bodySprites = new Sprite[32];
                    string[] dirs = { "Down", "Left", "Right", "Up" };
                    int idx = 0;
                    for (int d = 0; d < 4; d++)
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
