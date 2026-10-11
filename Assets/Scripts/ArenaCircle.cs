using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArenaCircle : MonoBehaviour
{
    [Header("Cercle de déplacement")]
    [SerializeField] private Transform circleCenter;
    [SerializeField, Min(0.01f)] private float radius = 20f;

    [Header("Apparence")]
    [SerializeField, Min(3)] private int segments = 100;
    [SerializeField, Min(0.001f)] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.white;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        DrawCircle();
    }

    private void OnValidate()
    {
        segments = Mathf.Max(3, segments);
        radius = Mathf.Max(0.01f, radius);

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null && circleCenter != null)
            DrawCircle();
    }

    private void DrawCircle()
    {
        if (circleCenter == null)
            return;

        lineRenderer.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 point = circleCenter.position + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            lineRenderer.SetPosition(i, point);
        }
    }
}