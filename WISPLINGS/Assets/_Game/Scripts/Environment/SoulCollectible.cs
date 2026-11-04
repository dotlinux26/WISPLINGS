using System;
using UnityEngine;
using GhostSurvival.Characters;

namespace GhostSurvival.Environment
{
    /// <summary>
    /// Represents a collectible spirit/soul in the world.
    /// Pulses, bobs, and gets absorbed when the player physically touches / collides with it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SoulCollectible : MonoBehaviour
    {
        [Header("Collection Settings")]
        [Tooltip("Soul value granted upon collection.")]
        [SerializeField] private int soulValue = 1;

        [Header("Visual Effects")]
        [SerializeField] private float bobSpeed = 3.0f;
        [SerializeField] private float bobHeight = 0.12f;
        [SerializeField] private GameObject collectionEffectPrefab;

        // Static event for game managers to listen to
        public static event Action<int> OnAnySoulCollected;
        public event Action<SoulCollectible> OnCollected;

        private Vector3 startPosition;
        private bool isCollected = false;

        private void Start()
        {
            startPosition = transform.position;

            // Ensure collider is a trigger
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (isCollected) return;

            // Subtle vertical floating
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            // Collect only when player physically touches the soul
            if (other.GetComponent<GhostController>() != null || other.GetComponentInParent<GhostController>() != null)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            OnCollected?.Invoke(this);
            OnAnySoulCollected?.Invoke(soulValue);

            Debug.Log($"[SoulCollectible] Collected soul (+{soulValue}) on direct touch at {transform.position}");

            if (collectionEffectPrefab != null)
            {
                Instantiate(collectionEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
