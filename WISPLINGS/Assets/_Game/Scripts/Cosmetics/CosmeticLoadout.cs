using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GhostSurvival.Cosmetics
{
    /// <summary>
    /// Manages equipped cosmetics (Hat, Wings, Hands) at the account level.
    /// Cosmetics are persistent loadout items, completely separate from expedition inventory.
    /// Supports runtime toggling via keyboard (H for Hat, J for Wings, K for Hands) or on-screen controls.
    /// </summary>
    public class CosmeticLoadout : MonoBehaviour
    {
        [Header("Cosmetic Renderers")]
        [SerializeField] private SpriteRenderer hatRenderer;
        [SerializeField] private SpriteRenderer wingsRenderer;
        [SerializeField] private SpriteRenderer handsRenderer;

        [Header("Account Profile Data (Local Mock)")]
        [SerializeField] private List<string> unlockedCosmeticIds = new List<string>
        {
            "cosmetic_hat_witch",
            "cosmetic_wings_moth",
            "cosmetic_hands_paws"
        };

        [SerializeField] private string equippedHatId = "cosmetic_hat_witch";
        [SerializeField] private string equippedWingsId = "cosmetic_wings_moth";
        [SerializeField] private string equippedHandsId = "cosmetic_hands_paws";

        [Header("Runtime Controls")]
        [Tooltip("Show simple on-screen buttons to toggle hat, wings and hands during prototype play.")]
        [SerializeField] private bool showDebugGui = true;

        public bool IsHatEquipped => !string.IsNullOrEmpty(equippedHatId) && hatRenderer != null && hatRenderer.enabled;
        public bool IsWingsEquipped => !string.IsNullOrEmpty(equippedWingsId) && wingsRenderer != null && wingsRenderer.enabled;
        public bool IsHandsEquipped => !string.IsNullOrEmpty(equippedHandsId) && handsRenderer != null && handsRenderer.enabled;

        private void Start()
        {
            ApplyEquippedCosmetics();
        }

        private void Update()
        {
            HandleInputToggle();
        }

        private void HandleInputToggle()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.hKey.wasPressedThisFrame)
                {
                    ToggleHat();
                }
                if (Keyboard.current.jKey.wasPressedThisFrame)
                {
                    ToggleWings();
                }
                if (Keyboard.current.kKey.wasPressedThisFrame)
                {
                    ToggleHands();
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleHat();
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                ToggleWings();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleHands();
            }
#endif
        }

        public void ApplyEquippedCosmetics()
        {
            if (hatRenderer != null)
            {
                hatRenderer.enabled = !string.IsNullOrEmpty(equippedHatId) && unlockedCosmeticIds.Contains(equippedHatId);
            }

            if (wingsRenderer != null)
            {
                wingsRenderer.enabled = !string.IsNullOrEmpty(equippedWingsId) && unlockedCosmeticIds.Contains(equippedWingsId);
            }

            if (handsRenderer != null)
            {
                handsRenderer.enabled = !string.IsNullOrEmpty(equippedHandsId) && unlockedCosmeticIds.Contains(equippedHandsId);
            }
        }

        public void ToggleHat()
        {
            if (string.IsNullOrEmpty(equippedHatId))
            {
                EquipHat("cosmetic_hat_witch");
            }
            else
            {
                UnequipHat();
            }
        }

        public void ToggleWings()
        {
            if (string.IsNullOrEmpty(equippedWingsId))
            {
                EquipWings("cosmetic_wings_moth");
            }
            else
            {
                UnequipWings();
            }
        }

        public void ToggleHands()
        {
            if (string.IsNullOrEmpty(equippedHandsId))
            {
                EquipHands("cosmetic_hands_paws");
            }
            else
            {
                UnequipHands();
            }
        }

        public void EquipHat(string hatId)
        {
            if (!unlockedCosmeticIds.Contains(hatId)) return;
            equippedHatId = hatId;
            if (hatRenderer != null) hatRenderer.enabled = true;
            Debug.Log($"[CosmeticLoadout] Equipped hat: {hatId}");
        }

        public void UnequipHat()
        {
            equippedHatId = "";
            if (hatRenderer != null) hatRenderer.enabled = false;
            Debug.Log("[CosmeticLoadout] Unequipped hat.");
        }

        public void EquipWings(string wingsId)
        {
            if (!unlockedCosmeticIds.Contains(wingsId)) return;
            equippedWingsId = wingsId;
            if (wingsRenderer != null) wingsRenderer.enabled = true;
            Debug.Log($"[CosmeticLoadout] Equipped wings: {wingsId}");
        }

        public void UnequipWings()
        {
            equippedWingsId = "";
            if (wingsRenderer != null) wingsRenderer.enabled = false;
            Debug.Log("[CosmeticLoadout] Unequipped wings.");
        }

        public void EquipHands(string handsId)
        {
            if (!unlockedCosmeticIds.Contains(handsId)) return;
            equippedHandsId = handsId;
            if (handsRenderer != null) handsRenderer.enabled = true;
            Debug.Log($"[CosmeticLoadout] Equipped hands: {handsId}");
        }

        public void UnequipHands()
        {
            equippedHandsId = "";
            if (handsRenderer != null) handsRenderer.enabled = false;
            Debug.Log("[CosmeticLoadout] Unequipped hands.");
        }

        private void OnGUI()
        {
            if (!showDebugGui) return;

            GUILayout.BeginArea(new Rect(10, 10, 230, 140));
            GUILayout.BeginVertical("box");
            GUILayout.Label("<b>W1SPL1NGS Cosmetics</b>");
            if (GUILayout.Button(IsHatEquipped ? "Remove Hat [H]" : "Equip Hat [H]"))
            {
                ToggleHat();
            }
            if (GUILayout.Button(IsWingsEquipped ? "Remove Wings [J]" : "Equip Wings [J]"))
            {
                ToggleWings();
            }
            if (GUILayout.Button(IsHandsEquipped ? "Remove Hands [K]" : "Equip Hands [K]"))
            {
                ToggleHands();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
