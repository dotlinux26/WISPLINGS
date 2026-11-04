using System;
using UnityEngine;
using GhostSurvival.Characters;

namespace GhostSurvival.Environment
{
    /// <summary>
    /// Represents a collectible spirit/soul in the world.
    /// Pulses, bobs, and gets absorbed when a player enters its collection radius.
    /// </summary>
    public class SoulCollectible : MonoBehaviour
    {
        [Header("Collection Settings")]
        [Tooltip("Distance at which the player collects this soul.")]
        [SerializeField] private float collectRadius = 1.2f;

        [Tooltip("Soul value granted upon collection.")]
        [SerializeField] private int soulValue = 1;

        [Header("Visual Effects")]
        [SerializeField] private float bobSpeed = 3.0f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private GameObject collectionEffectPrefab;

        // Static event for game managers to listen to
        public static event Action<int> OnAnySoulCollected;
        public event Action<SoulCollectible> OnCollected;

        private Vector3 startPosition;
        private bool isCollected = false;
        private Transform playerTransform;

        private void Start()
        {
            startPosition = transform.position;
            FindPlayer();
        }

        private void Update()
        {
            if (isCollected) return;

            // Subtle vertical floating
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Proximity detection
            if (playerTransform == null)
            {
                FindPlayer();
            }

            if (playerTransform != null)
            {
                float distSqr = (transform.position - playerTransform.position).sqrMagnitude;
                if (distSqr <= collectRadius * collectRadius)
                {
                    Collect();
                }
            }
        }

        private void FindPlayer()
        {
            var player = FindFirstObjectByType<GhostController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        public void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            OnCollected?.Invoke(this);
            OnAnySoulCollected?.Invoke(soulValue);

            Debug.Log($"[SoulCollectible] Collected soul (+{soulValue}) at {transform.position}");

            if (collectionEffectPrefab != null)
            {
                Instantiate(collectionEffectPrefab, transform.position, Quaternion.identity);
            }

            // Disable or destroy object
            Destroy(gameObject, 0.05f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.75f, 0.9f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, collectRadius);
        }
    }
}

