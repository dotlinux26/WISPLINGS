using System.Collections.Generic;
using UnityEngine;

namespace GhostSurvival.Cosmetics
{
    /// <summary>
    /// Synchronizes cosmetic layer sprites (Hat, Wings) directly with the body sprite frame.
    /// Because the body, hat, and wings atlases share the exact same 8x4 layout (Down, Left, Right, Up x Idle 0..3, Walk 0..3),
    /// matching the body's frame index guarantees frame-perfect alignment without animator desynchronization.
    /// </summary>
    public class CosmeticSynchronizer : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer hatRenderer;
        [SerializeField] private SpriteRenderer wingsRenderer;

        [Header("Sprite Lookups (Indexed 0..31 matching 8x4 atlas)")]
        [SerializeField] private Sprite[] hatSprites;
        [SerializeField] private Sprite[] wingsSprites;

        private Sprite lastBodySprite;
        private readonly Dictionary<string, int> bodySpriteNameToIndex = new Dictionary<string, int>();

        private void Awake()
        {
            InitializeSpriteIndexMap();
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

        public void SetLookups(Sprite[] hats, Sprite[] wings)
        {
            hatSprites = hats;
            wingsSprites = wings;
            InitializeSpriteIndexMap();
        }

        private void InitializeSpriteIndexMap()
        {
            bodySpriteNameToIndex.Clear();
            string[] directions = { "Down", "Left", "Right", "Up" };

            int index = 0;
            for (int r = 0; r < 4; r++)
            {
                string dir = directions[r];
                // Columns 0..3: Idle
                for (int c = 0; c < 4; c++)
                {
                    bodySpriteNameToIndex[$"Ghost_{dir}_Idle_{c}"] = index++;
                }
                // Columns 4..7: Walk
                for (int c = 0; c < 4; c++)
                {
                    bodySpriteNameToIndex[$"Ghost_{dir}_Walk_{c}"] = index++;
                }
            }
        }

        private void SyncCosmetics(string bodySpriteName)
        {
            if (!bodySpriteNameToIndex.TryGetValue(bodySpriteName, out int index))
            {
                // Fallback: extract last number or substring if renamed
                return;
            }

            if (hatRenderer != null && hatRenderer.enabled && hatSprites != null && index < hatSprites.Length)
            {
                hatRenderer.sprite = hatSprites[index];
            }

            if (wingsRenderer != null && wingsRenderer.enabled && wingsSprites != null && index < wingsSprites.Length)
            {
                wingsRenderer.sprite = wingsSprites[index];
            }
        }
    }
}
