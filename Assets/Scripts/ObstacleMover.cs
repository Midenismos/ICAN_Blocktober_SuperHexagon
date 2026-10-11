using System.Collections;
using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public enum ArrivalBehavior
    {
        ContinueStraight,
        Stop,
        TurnBack,
        Destroy
    }

    [Header("Pré-annonce")]
    [SerializeField, Min(0f)]
    private float warningDuration = 2f;

    [SerializeField, Range(0f, 1f)]
    private float warningAlpha = 0.35f;

    [Header("Déplacement")]
    [SerializeField, Min(0f)]
    private float speed = 2f;

    [Header("Durée de vie")]
    [Tooltip("-1 = illimitée jusqu'à la sortie de l'écran.")]
    [SerializeField]
    private float lifetime = -1f;

    [Header("Hors écran")]
    [SerializeField, Min(0f)]
    private float screenMargin = 1f;

    private Vector2 targetPosition;
    private Transform followTarget;
    private bool followsTarget;
    private ArrivalBehavior arrivalBehavior;

    private Vector2 travelDirection;
    private bool hasReachedTarget;
    private bool configured;
    private bool isMoving;

    private Camera mainCamera;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Collider2D[] obstacleColliders;
    private bool[] collidersWereEnabled;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
            originalColors[i] = spriteRenderers[i].color;

        obstacleColliders = GetComponentsInChildren<Collider2D>(true);
        collidersWereEnabled = new bool[obstacleColliders.Length];

        for (int i = 0; i < obstacleColliders.Length; i++)
            collidersWereEnabled[i] = obstacleColliders[i].enabled;
    }

    /// <summary>
    /// Appelé par ObstacleSpawner juste après la création de l'obstacle.
    /// </summary>
    public void Configure(
        Vector2 target,
        Transform targetToFollow,
        bool shouldFollowTarget,
        float movementSpeed,
        float obstacleScreenMargin,
        float obstacleLifetime,
        ArrivalBehavior behavior,
        float warningTime,
        float transparentAlpha)
    {
        targetPosition = target;
        followTarget = targetToFollow;
        followsTarget = shouldFollowTarget;
        speed = movementSpeed;
        screenMargin = obstacleScreenMargin;
        lifetime = obstacleLifetime;
        arrivalBehavior = behavior;
        warningDuration = warningTime;
        warningAlpha = transparentAlpha;
        configured = true;
    }

    private void Start()
    {
        if (!configured)
        {
            Debug.LogError(
                "ObstacleMover : Configure n'a pas été appelé par ObstacleSpawner.",
                this
            );
            enabled = false;
            return;
        }

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "ObstacleMover : aucune caméra avec le tag MainCamera n'a été trouvée.",
                this
            );
            enabled = false;
            return;
        }

        if (lifetime > 0f)
            Destroy(gameObject, warningDuration + lifetime);

        SetCollidersEnabled(false);
        SetAlpha(warningAlpha);

        StartCoroutine(WarningThenMove());
    }

    private IEnumerator WarningThenMove()
    {
        if (warningDuration > 0f)
            yield return new WaitForSeconds(warningDuration);

        SetAlpha(1f);
        SetCollidersEnabled(true);

        isMoving = true;
        UpdateDirectionToTarget();
    }

    private void Update()
    {
        if (!isMoving)
            return;

        if (followsTarget && followTarget != null && !hasReachedTarget)
            targetPosition = followTarget.position;

        if (!hasReachedTarget)
            UpdateDirectionToTarget();

        transform.position += (Vector3)(travelDirection * speed * Time.deltaTime);

        if (!hasReachedTarget && HasReachedTarget())
            HandleArrival();

        if (lifetime <= 0f && IsOutsideCamera())
            Destroy(gameObject);
    }

    private void UpdateDirectionToTarget()
    {
        Vector2 toTarget = targetPosition - (Vector2)transform.position;

        if (toTarget.sqrMagnitude > 0.0001f)
            travelDirection = toTarget.normalized;
    }

    private bool HasReachedTarget()
    {
        Vector2 toTarget = targetPosition - (Vector2)transform.position;
        return toTarget.sqrMagnitude < 0.05f;
    }

    private void HandleArrival()
    {
        hasReachedTarget = true;

        switch (arrivalBehavior)
        {
            case ArrivalBehavior.ContinueStraight:
                break;

            case ArrivalBehavior.Stop:
                speed = 0f;
                break;

            case ArrivalBehavior.TurnBack:
                travelDirection = -travelDirection;
                hasReachedTarget = false;
                targetPosition = GetPointBehindStart();
                break;

            case ArrivalBehavior.Destroy:
                Destroy(gameObject);
                break;
        }
    }

    private Vector2 GetPointBehindStart()
    {
        return (Vector2)transform.position + travelDirection * 1000f;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderers == null)
            return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
                continue;

            Color color = originalColors[i];
            color.a *= alpha;
            spriteRenderers[i].color = color;
        }
    }

    private void SetCollidersEnabled(bool shouldEnable)
    {
        if (obstacleColliders == null)
            return;

        for (int i = 0; i < obstacleColliders.Length; i++)
        {
            if (obstacleColliders[i] != null)
                obstacleColliders[i].enabled =
                    shouldEnable && collidersWereEnabled[i];
        }
    }

    private bool IsOutsideCamera()
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        return viewportPosition.x < -screenMargin ||
               viewportPosition.x > 1f + screenMargin ||
               viewportPosition.y < -screenMargin ||
               viewportPosition.y > 1f + screenMargin;
    }
}