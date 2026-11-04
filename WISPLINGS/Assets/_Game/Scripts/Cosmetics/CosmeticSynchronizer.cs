using System.Collections.Generic;
using UnityEngine;

namespace GhostSurvival.Cosmetics
{
    /// <summary>
    /// Synchronizes cosmetic layer sprites (Hat, Wings) directly with the body sprite frame.
    /// Ensures hat and wings follow every single walk/idle frame, bobbing, and directional turn.
    /// Guarantees exact frame mapping via dictionaries and canonical 32-element arrays.
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
        [SerializeField] private Vector3 wingsOffset = new Vector3(0f, 0.08f, 0f);

        [Tooltip("Local scale for the wings (makes moth wings larger and more majestic).")]
        [SerializeField] private Vector3 wingsScale = new Vector3(1.35f, 1.35f, 1f);

        [Header("Sprite Lookups (32 frames matching 8x4 atlas)")]
        [SerializeField] private Sprite[] hatSprites = new Sprite[32];
        [SerializeField] private Sprite[] wingsSprites = new Sprite[32];

        private Sprite lastBodySprite;
        private readonly Dictionary<string, Sprite> hatSpriteMap = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Sprite> wingsSpriteMap = new Dictionary<string, Sprite>();

        public Vector3 HatOffset
        {
            get => hatOffset;
            set
            {
                hatOffset = value;
                ApplyOffsets();
            }
        }

        public Vector3 WingsOffset
        {
            get => wingsOffset;
            set
            {
                wingsOffset = value;
                ApplyOffsets();
            }
        }

        public Vector3 WingsScale
        {
            get => wingsScale;
            set
            {
                wingsScale = value;
                ApplyOffsets();
            }
        }

        private void Awake()
        {
            ResolveRenderers();
            EnsureSpritesLoaded();
            InitializeSpriteMaps();
            ApplyOffsets();
        }

        private void Start()
        {
            ApplyOffsets();
            string initialSpriteName = (bodyRenderer != null && bodyRenderer.sprite != null)
                ? bodyRenderer.sprite.name
                : "Ghost_Down_Idle_0";
            SyncCosmetics(initialSpriteName);
        }

        private void OnValidate()
        {
            ApplyOffsets();
        }

        public void ResolveRenderers()
        {
            if (bodyRenderer == null || hatRenderer == null || wingsRenderer == null)
            {
                var renderers = GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var r in renderers)
                {
                    if (bodyRenderer == null && r.gameObject.name.Contains("Body"))
                        bodyRenderer = r;
                    else if (hatRenderer == null && r.gameObject.name.Contains("Hat"))
                        hatRenderer = r;
                    else if (wingsRenderer == null && r.gameObject.name.Contains("Wings"))
                        wingsRenderer = r;
                }
            }
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
                wingsRenderer.transform.localScale = wingsScale;
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
            LoadCategorySprites("Assets/_Game/Art/Sprites/cosmetic_hat_witch_8x4.png", "Hat_Witch", ref hatSprites, hatSpriteMap);
            LoadCategorySprites("Assets/_Game/Art/Sprites/cosmetic_wings_moth_8x4.png", "Wings_Moth", ref wingsSprites, wingsSpriteMap);
#endif
        }

#if UNITY_EDITOR
        private void LoadCategorySprites(string atlasPath, string prefix, ref Sprite[] array, Dictionary<string, Sprite> map)
        {
            if (array == null || array.Length < 32 || array[0] == null)
            {
                var dict = new Dictionary<string, Sprite>();
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(atlasPath))
                {
                    if (obj is Sprite s && s.name.StartsWith(prefix))
                    {
                        dict[s.name] = s;
                        map[s.name] = s;
                    }
                }

                if (dict.Count >= 32)
                {
                    array = new Sprite[32];
                    string[] dirs = { "Down", "Left", "Right", "Up" };
                    int idx = 0;
                    for (int d = 0; d < 4; d++)
                    {
                        string dir = dirs[d];
                        for (int f = 0; f < 4; f++)
                        {
                            dict.TryGetValue($"{prefix}_{dir}_Idle_{f}", out array[idx]);
                            idx++;
                        }
                        for (int f = 0; f < 4; f++)
                        {
                            dict.TryGetValue($"{prefix}_{dir}_Walk_{f}", out array[idx]);
                            idx++;
                        }
                    }
                }
            }
        }
#endif

        private void InitializeSpriteMaps()
        {
            PopulateMapFromList(hatSprites, "Hat_Witch", hatSpriteMap);
            PopulateMapFromList(wingsSprites, "Wings_Moth", wingsSpriteMap);

#if UNITY_EDITOR
            if (hatSpriteMap.Count < 32)
            {
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/cosmetic_hat_witch_8x4.png"))
                {
                    if (obj is Sprite s && s.name.StartsWith("Hat_Witch"))
                    {
                        hatSpriteMap[s.name] = s;
                    }
                }
            }
            if (wingsSpriteMap.Count < 32)
            {
                foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/cosmetic_wings_moth_8x4.png"))
                {
                    if (obj is Sprite s && s.name.StartsWith("Wings_Moth"))
                    {
                        wingsSpriteMap[s.name] = s;
                    }
                }
            }
#endif
        }

        private void PopulateMapFromList(Sprite[] sprites, string prefix, Dictionary<string, Sprite> map)
        {
            if (sprites == null) return;
            foreach (var s in sprites)
            {
                if (s != null && s.name.StartsWith(prefix))
                {
                    map[s.name] = s;
                }
            }
        }

        public void SyncCosmetics(string bodySpriteName)
        {
            // 1. Hat Witch: Ghost_Down_Walk_1 -> Hat_Witch_Down_Walk_1
            if (hatRenderer != null && hatRenderer.enabled)
            {
                string targetHatName = bodySpriteName.Replace("Ghost_", "Hat_Witch_");
                if (hatSpriteMap.TryGetValue(targetHatName, out Sprite matchedHat))
                {
                    hatRenderer.sprite = matchedHat;
                }
                else if (hatSpriteMap.TryGetValue("Hat_Witch_Down_Idle_0", out Sprite fallbackHat))
                {
                    hatRenderer.sprite = fallbackHat;
                }
            }

            // 2. Wings Moth: Ghost_Down_Walk_1 -> Wings_Moth_Down_Walk_1
            if (wingsRenderer != null && wingsRenderer.enabled)
            {
                string targetWingsName = bodySpriteName.Replace("Ghost_", "Wings_Moth_");
                if (wingsSpriteMap.TryGetValue(targetWingsName, out Sprite matchedWings))
                {
                    wingsRenderer.sprite = matchedWings;
                }
                else if (wingsSpriteMap.TryGetValue("Wings_Moth_Down_Idle_0", out Sprite fallbackWings))
                {
                    wingsRenderer.sprite = fallbackWings;
                }
            }
        }
    }
}
