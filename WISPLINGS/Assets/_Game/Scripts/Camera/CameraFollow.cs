using UnityEngine;
using GhostSurvival.Characters;

namespace GhostSurvival.Cameras
{
    /// <summary>
    /// Smooth 2D camera follower that tracks the player ghost without jitter.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Smoothing Settings")]
        [Tooltip("Approximate time to reach the target.")]
        [SerializeField] private float smoothTime = 0.2f;

        private Vector3 currentVelocity = Vector3.zero;

        private void Start()
        {
            if (target == null)
            {
                var player = FindFirstObjectByType<GhostController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
