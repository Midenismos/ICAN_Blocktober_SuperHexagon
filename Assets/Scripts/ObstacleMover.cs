using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public enum ArrivalBehavior
    {
        ContinueStraight,
        Stop,
        Reverse,
        Disappear
    }

    [SerializeField, Min(0f)] private float speed = 2f;
    [SerializeField, Min(0f)] private float screenMargin = 1f;

    private Vector2 targetPosition;
    private Transform followedPlayer;
    private bool followPlayer;
    private ArrivalBehavior arrivalBehavior;
    private float lifetime;

    private Vector2 travelDirection;
    private bool hasReachedTarget;
    private Camera mainCamera;
    private bool configured;

    private void Start()
    {
        if (!configured)
        {
            Debug.LogError(
                "ObstacleMover : cet obstacle doit être configuré par ObstacleSpawner.",
                this
            );
            enabled = false;
            return;
        }

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);

        UpdateDirectionToTarget();

        if (travelDirection == Vector2.zero)
        {
            Debug.LogError(
                "ObstacleMover : l’obstacle et sa cible sont au même endroit.",
                this
            );
            enabled = false;
            return;
        }

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "ObstacleMover : aucune caméra avec le tag MainCamera n’a été trouvée.",
                this
            );
            enabled = false;
        }
    }

    private void Update()
    {
        if (followPlayer && followedPlayer != null)
        {
            targetPosition = followedPlayer.position;
            UpdateDirectionToTarget();
        }

        if (arrivalBehavior == ArrivalBehavior.Stop && hasReachedTarget)
            return;

        transform.position += (Vector3)(travelDirection * speed * Time.deltaTime);

        if (!hasReachedTarget && HasPassedTarget())
        {
            hasReachedTarget = true;

            switch (arrivalBehavior)
            {
                case ArrivalBehavior.Stop:
                    transform.position = targetPosition;
                    return;

                case ArrivalBehavior.Reverse:
                    travelDirection = -travelDirection;
                    break;

                case ArrivalBehavior.Disappear:
                    Destroy(gameObject);
                    return;

                case ArrivalBehavior.ContinueStraight:
                    break;
            }
        }

        DestroyIfOutsideCamera();
    }

    private void UpdateDirectionToTarget()
    {
        Vector2 toTarget = targetPosition - (Vector2)transform.position;

        if (toTarget.sqrMagnitude > 0.0001f)
            travelDirection = toTarget.normalized;
    }

    private bool HasPassedTarget()
    {
        Vector2 toTarget = targetPosition - (Vector2)transform.position;

        // Positif tant que l’obstacle n’a pas encore atteint ou dépassé la cible.
        return Vector2.Dot(toTarget, travelDirection) <= 0f;
    }

    private void DestroyIfOutsideCamera()
    {
        if (mainCamera == null)
            return;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPosition.x < -screenMargin ||
            viewportPosition.x > 1f + screenMargin ||
            viewportPosition.y < -screenMargin ||
            viewportPosition.y > 1f + screenMargin)
        {
            Destroy(gameObject);
        }
    }

    public void Configure(
        Vector2 target,
        Transform player,
        bool shouldFollowPlayer,
        float movementSpeed,
        float obstacleScreenMargin,
        float obstacleLifetime,
        ArrivalBehavior behavior)
    {
        targetPosition = target;
        followedPlayer = player;
        followPlayer = shouldFollowPlayer && player != null;
        speed = movementSpeed;
        screenMargin = obstacleScreenMargin;
        lifetime = obstacleLifetime;
        arrivalBehavior = behavior;
        configured = true;
    }
}