using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using GhostSurvival.Characters;

namespace GhostSurvival.Cameras
{
    /// <summary>
    /// Smooth 2D camera follower that tracks the player ghost without jitter.
    /// Supports zooming in/out via Mouse Scroll Wheel and keys I (in) / O (out), with min/max clamps.
    /// Uses New Input System by default without throwing InvalidOperationException.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Tracking Smoothness")]
        [Tooltip("Approximate time to reach the target.")]
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Camera Zoom Settings")]
        [Tooltip("Minimum orthographic size (closest zoom).")]
        [SerializeField] private float minZoom = 2.0f;

        [Tooltip("Maximum orthographic size (farthest zoom).")]
        [SerializeField] private float maxZoom = 7.5f;

        [Tooltip("Current target zoom size.")]
        [SerializeField] private float targetZoom = 4.0f;

        [Tooltip("Zoom sensitivity when scrolling.")]
        [SerializeField] private float scrollSensitivity = 1.0f;

        [Tooltip("Zoom speed when pressing I / O keys.")]
        [SerializeField] private float keyZoomSpeed = 3.5f;

        [Tooltip("Smoothness of zoom transition.")]
        [SerializeField] private float zoomSmoothTime = 0.15f;

        private Camera cam;
        private Vector3 currentVelocity = Vector3.zero;
        private float zoomVelocity = 0f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                targetZoom = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
        }

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

        private void Update()
        {
            HandleZoomInput();
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 targetPosition = target.position + offset;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            }

            if (cam != null)
            {
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
            }
        }

        private void HandleZoomInput()
        {
            float scrollDelta = 0f;
            float keyDelta = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                scrollDelta = Mouse.current.scroll.ReadValue().y;
            }
            if (Keyboard.current != null)
            {
                if (Keyboard.current.iKey.isPressed) keyDelta -= 1f; // Zoom in
                if (Keyboard.current.oKey.isPressed) keyDelta += 1f; // Zoom out
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                scrollDelta = Input.mouseScrollDelta.y;
            }
            catch { }

            if (Input.GetKey(KeyCode.I)) keyDelta -= 1f;
            if (Input.GetKey(KeyCode.O)) keyDelta += 1f;
#endif

            // Apply scroll delta (scroll up > 0 zooms in -> decrease size)
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                float scrollStep = Mathf.Sign(scrollDelta) * scrollSensitivity;
                targetZoom -= scrollStep;
            }

            // Apply key zoom delta
            if (Mathf.Abs(keyDelta) > 0.01f)
            {
                targetZoom += keyDelta * keyZoomSpeed * Time.deltaTime;
            }

            // Clamp zoom within safe boundaries
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
