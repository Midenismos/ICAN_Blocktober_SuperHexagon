using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Victory,
        GameOver
    }

    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;

    public GameConfig Config => gameConfig;
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("Condition de victoire")]
    [SerializeField, Min(1f)] private float survivalTimeToWin = 60f;

    [Header("Événements")]
    [SerializeField] private UnityEvent onVictory;
    [SerializeField] private UnityEvent onGameOver;

    private float elapsedTime;

    private void Awake()
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig n'est pas assigné au GameManager.", this);
        }

        // Utile si une partie se termine puis que la scène est relancée.
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= survivalTimeToWin)
            Win();
    }

    public void Win()
    {
        EndGame(GameState.Victory);
    }

    public void GameOver()
    {
        EndGame(GameState.GameOver);
    }

    private void EndGame(GameState finalState)
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = finalState;

        if (finalState == GameState.Victory)
            onVictory?.Invoke();
        else
            onGameOver?.Invoke();

        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void OnValidate()
    {
        survivalTimeToWin = Mathf.Max(1f, survivalTimeToWin);
    }
}