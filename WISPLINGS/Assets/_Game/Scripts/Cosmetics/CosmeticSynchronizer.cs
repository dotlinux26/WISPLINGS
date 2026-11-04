using System.Collections.Generic;
using UnityEngine;

namespace GhostSurvival.Cosmetics
{
    /// <summary>
    /// Synchronizes cosmetic layer sprites (Hat, Wings) directly with the body sprite frame.
    /// Ensures hat and wings follow every single walk/idle frame, bobbing, and directional turn.
    /// Supports automatic sprite caching and name-based / index-based frame locking.
    /// </summary>
    public class CosmeticSynchronizer : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer hatRenderer;
        [SerializeField] private SpriteRenderer wingsRenderer;

        [Header("Position Offsets")]
        [Tooltip("Local position offset for the hat (higher Y lifts the hat nicely above the eyes).")]
        [SerializeField] private Vector3 hatOffset = new Vector3(0f, 0.22f, 0f);

        [Tooltip("Local position offset for the wings.")]
        [SerializeField] private Vector3 wingsOffset = new Vector3(0f, 0.05f, 0f);

        [Header("Sprite Lookups (32 frames matching 8x4 atlas)")]
        [SerializeField] private Sprite[] hatSprites = new Sprite[32];
        [SerializeField] private Sprite[] wingsSprites = new Sprite[32];

        private Sprite lastBodySprite;
        private readonly Dictionary<string, Sprite> hatSpriteMap = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Sprite> wingsSpriteMap = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, int> bodySpriteNameToIndex = new Dictionary<string, int>();

        public Vector3 HatOffset
        {
            get => hatOffset;
            set
            {
                hatOffset = value;
                ApplyOffsets();
            }
        }

        private void Awake()
        {
            EnsureSpritesLoaded();
            InitializeSpriteMaps();
            ApplyOffsets();
        }

        private void Start()
        {
            ApplyOffsets();
            if (bodyRenderer != null && bodyRenderer.sprite != null)
            {
                SyncCosmetics(bodyRenderer.sprite.name);
            }
        }

        private void OnValidate()
        {
            ApplyOffsets();
        }

        public void ApplyOffsets()
        {
            if (hatRenderer != null)
            {
                hatRenderer.transform.localPosition = hatOffset;
            }
            if (wingsRenderer != null)
            {
                wingsRenderer.transform.localPosition = wingsOffset;
            }
        }

        private void LateUpdate()
        {
            if (bodyRenderer == null || bodyRenderer.sprite == null) return;

            Sprite currentBodySprite = bodyRenderer.sprite;
            if (currentBodySprite != lastBodySprite)
            {
                lastBodySprite = currentBodySprite;
                SyncCosmetics(currentBodySprite.name);
            }
        }

        private void EnsureSpritesLoaded()
        {
#if UNITY_EDITOR
            if (hatSprites == null || hatSprites.Length == 0 || hatSprites[0] == null)
            {
                var loadedHats = new List<Sprite>();
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/cosmetic_hat_witch_8x4.png"))
                {
                    if (obj is Sprite s) loadedHats.Add(s);
                }
                if (loadedHats.Count > 0) hatSprites = loadedHats.ToArray();
            }

            if (wingsSprites == null || wingsSprites.Length == 0 || wingsSprites[0] == null)
            {
                var loadedWings = new List<Sprite>();
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/cosmetic_wings_moth_8x4.png"))
                {
                    if (obj is Sprite s) loadedWings.Add(s);
                }
                if (loadedWings.Count > 0) wingsSprites = loadedWings.ToArray();
            }
#endif
        }

        private void InitializeSpriteMaps()
        {
            hatSpriteMap.Clear();
            wingsSpriteMap.Clear();
            bodySpriteNameToIndex.Clear();

            string[] directions = { "Down", "Left", "Right", "Up" };
            int index = 0;
            for (int r = 0; r < 4; r++)
            {
                string dir = directions[r];
                for (int c = 0; c < 4; c++)
                {
                    bodySpriteNameToIndex[$"Ghost_{dir}_Idle_{c}"] = index++;
                }
                for (int c = 0; c < 4; c++)
                {
                    bodySpriteNameToIndex[$"Ghost_{dir}_Walk_{c}"] = index++;
                }
            }

            if (hatSprites != null)
            {
                foreach (var s in hatSprites)
                {
                    if (s != null) hatSpriteMap[s.name] = s;
                }
            }

            if (wingsSprites != null)
            {
                foreach (var s in wingsSprites)
                {
                    if (s != null) wingsSpriteMap[s.name] = s;
                }
            }
        }

        private void SyncCosmetics(string bodySpriteName)
        {
            // 1. Try direct name conversion: Ghost_Down_Walk_1 -> Hat_Witch_Down_Walk_1
            string targetHatName = bodySpriteName.Replace("Ghost_", "Hat_Witch_");
            if (hatRenderer != null && hatRenderer.enabled)
            {
                if (hatSpriteMap.TryGetValue(targetHatName, out Sprite matchedHat))
                {
                    hatRenderer.sprite = matchedHat;
                }
                else if (bodySpriteNameToIndex.TryGetValue(bodySpriteName, out int idx) && hatSprites != null && idx >= 0 && idx < hatSprites.Length)
                {
                    if (hatSprites[idx] != null) hatRenderer.sprite = hatSprites[idx];
                }
            }

            // 2. Wings conversion: Ghost_Down_Walk_1 -> Wings_Moth_Down_Walk_1
            string targetWingsName = bodySpriteName.Replace("Ghost_", "Wings_Moth_");
            if (wingsRenderer != null && wingsRenderer.enabled)
            {
                if (wingsSpriteMap.TryGetValue(targetWingsName, out Sprite matchedWings))
                {
                    wingsRenderer.sprite = matchedWings;
                }
                else if (bodySpriteNameToIndex.TryGetValue(bodySpriteName, out int idx) && wingsSprites != null && idx >= 0 && idx < wingsSprites.Length)
                {
                    if (wingsSprites[idx] != null) wingsRenderer.sprite = wingsSprites[idx];
                }
            }
        }
    }
}
