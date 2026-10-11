using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public enum TargetType
    {
        ArenaCenter,
        Player,
        ScenePoint
    }

    public enum PlayerTargetMode
    {
        FollowPlayer,
        SnapshotAtSpawn
    }

    [System.Serializable]
    private class ObstacleType
    {
        [Header("Obstacle")]
        public GameObject prefab;
        public Vector2 scale = Vector2.one;

        [Min(0f)]
        public float speed = 2f;

        [Tooltip("Durée de vie en secondes. -1 signifie illimitée jusqu'à la sortie de l'écran.")]
        public float lifetime = -1f;

        [Header("Pré-annonce")]
        [Min(0f)]
        public float warningDuration = 2f;

        [Range(0f, 1f)]
        public float warningAlpha = 0.35f;

        [Header("Apparition")]
        public bool randomAngle = true;

        [Range(0f, 360f)]
        public float angle;

        [Header("Cible")]
        public TargetType targetType = TargetType.ArenaCenter;
        public PlayerTargetMode playerTargetMode = PlayerTargetMode.SnapshotAtSpawn;
        public Transform scenePoint;

        [Header("À l'arrivée")]
        public ObstacleMover.ArrivalBehavior arrivalBehavior =
            ObstacleMover.ArrivalBehavior.ContinueStraight;
    }

    [Header("Types d'obstacles")]
    [SerializeField] private ObstacleType[] obstacleTypes;

    [Header("Références")]
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private Transform player;

    [Header("Apparition")]
    [SerializeField, Min(0.1f)] private float spawnRadius = 8f;
    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
    [SerializeField, Min(0f)] private float screenMargin = 1f;

    private float timer;
    private GameManager gameManager;

    private void Start()
    {
        if (obstacleTypes == null || obstacleTypes.Length == 0)
        {
            Debug.LogError("ObstacleSpawner : ajoute au moins un type d'obstacle.", this);
            enabled = false;
            return;
        }

        if (arenaCenter == null)
        {
            Debug.LogError("ObstacleSpawner : assigne ArenaCenter.", this);
            enabled = false;
            return;
        }

        gameManager = FindFirstObjectByType<GameManager>();
        timer = spawnInterval;
    }

    private void Update()
    {
        if (gameManager != null &&
            gameManager.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnObstacle();
            timer = spawnInterval;
        }
    }

    private void SpawnObstacle()
    {
        ObstacleType type = obstacleTypes[Random.Range(0, obstacleTypes.Length)];

        if (type.prefab == null)
        {
            Debug.LogError("ObstacleSpawner : un type d'obstacle n'a pas de prefab.", this);
            return;
        }

        Transform playerToFollow = null;
        Vector2 targetPosition;

        switch (type.targetType)
        {
            case TargetType.ArenaCenter:
                targetPosition = arenaCenter.position;
                break;

            case TargetType.Player:
                if (player == null)
                {
                    Debug.LogError("ObstacleSpawner : assigne le Player dans l'Inspector.", this);
                    return;
                }

                targetPosition = player.position;

                if (type.playerTargetMode == PlayerTargetMode.FollowPlayer)
                    playerToFollow = player;

                break;

            case TargetType.ScenePoint:
                if (type.scenePoint == null)
                {
                    Debug.LogError(
                        "ObstacleSpawner : assigne un Scene Point pour ce type d'obstacle.",
                        this
                    );
                    return;
                }

                targetPosition = type.scenePoint.position;
                break;

            default:
                targetPosition = arenaCenter.position;
                break;
        }

        float angle = type.randomAngle ? Random.Range(0f, 360f) : type.angle;
        float radians = angle * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        Vector3 spawnPosition =
            (Vector3)targetPosition + (Vector3)(direction * spawnRadius);

        GameObject obstacle = Instantiate(
            type.prefab,
            spawnPosition,
            Quaternion.identity
        );

        obstacle.transform.localScale = new Vector3(type.scale.x, type.scale.y, 1f);

        ObstacleMover mover = obstacle.GetComponent<ObstacleMover>();

        if (mover == null)
        {
            Debug.LogError(
                $"Le prefab « {type.prefab.name} » n'a pas de composant ObstacleMover.",
                obstacle
            );
            Destroy(obstacle);
            return;
        }

        mover.Configure(
            targetPosition,
            playerToFollow,
            playerToFollow != null,
            type.speed,
            screenMargin,
            type.lifetime,
            type.arrivalBehavior,
            type.warningDuration,
            type.warningAlpha
        );
    }
}