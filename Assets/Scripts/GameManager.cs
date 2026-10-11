using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Condition de victoire provisoire")]
    [SerializeField, Min(1f)] private float victoryTime = 60f;

    [Header("Écrans de fin")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    private float elapsedTime;

    public GameConfig Config => gameConfig;
    public GameState CurrentState { get; private set; } = GameState.Playing;

    private void Awake()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Playing;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        elapsedTime += Time.deltaTime;

        // Condition provisoire : gagner en survivant jusqu'à la durée indiquée.
        if (elapsedTime >= victoryTime)
            Victory();
    }

    public void GameOver()
    {
        EndGame(GameState.GameOver, gameOverPanel);
    }

    public void Victory()
    {
        EndGame(GameState.Victory, victoryPanel);
    }

    private void EndGame(GameState state, GameObject panel)
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = state;

        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning($"GameManager : le panneau pour l'état {state} n'est pas assigné.", this);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}