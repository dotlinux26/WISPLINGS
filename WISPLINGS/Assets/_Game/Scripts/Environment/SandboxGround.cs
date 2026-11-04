using UnityEngine;

namespace GhostSurvival.Environment
{
    /// <summary>
    /// Renders a simple, clean atmospheric grayscale ground surface for the ArtSandbox prototype.
    /// Uses low-saturation dark charcoal tones matching the art direction.
    /// </summary>
    public class SandboxGround : MonoBehaviour
    {
        [SerializeField] private int width = 30;
        [SerializeField] private int height = 30;
        [SerializeField] private Color groundColor = new Color(0.12f, 0.12f, 0.15f, 1f);
        [SerializeField] private Color gridColor = new Color(0.16f, 0.16f, 0.20f, 1f);

        private void OnDrawGizmos()
        {
            Gizmos.color = groundColor;
            Gizmos.DrawCube(transform.position, new Vector3(width, height, 0.1f));
        }
    }
}

