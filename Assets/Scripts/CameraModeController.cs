using UnityEngine;

[DisallowMultipleComponent]
public class CameraModeController : MonoBehaviour
{
    public enum CameraMode
    {
        Static,
        VerticalAutoscroll,
        HorizontalAutoscroll
    }

    [Header("Mode de caméra")]
    [SerializeField] private CameraMode mode = CameraMode.Static;

    [Header("Autoscroll")]
    [Tooltip("Vitesse de déplacement de la caméra en unités Unity par seconde.")]
    [SerializeField, Min(0f)] private float scrollSpeed = 2f;

    [Tooltip("Si activé, la caméra repart au point de départ lorsqu'elle atteint la distance configurée.")]
    [SerializeField] private bool useDistanceLimit;

    [Tooltip("Distance parcourue avant le retour au point de départ. Utilisé uniquement si Use Distance Limit est activé.")]
    [SerializeField, Min(0f)] private float distanceLimit = 20f;

    private Vector3 startPosition;
    private float distanceTravelled;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (mode == CameraMode.Static || scrollSpeed <= 0f)
            return;

        Vector3 direction = mode == CameraMode.VerticalAutoscroll
            ? Vector3.up
            : Vector3.right;

        float distanceThisFrame = scrollSpeed * Time.deltaTime;
        transform.position += direction * distanceThisFrame;
        distanceTravelled += distanceThisFrame;

        if (useDistanceLimit && distanceTravelled >= distanceLimit)
        {
            transform.position = startPosition;
            distanceTravelled = 0f;
        }
    }

    /// <summary>
    /// Permet à un autre script de changer le mode pendant le jeu.
    /// </summary>
    public void SetMode(CameraMode newMode)
    {
        mode = newMode;
        startPosition = transform.position;
        distanceTravelled = 0f;
    }
}