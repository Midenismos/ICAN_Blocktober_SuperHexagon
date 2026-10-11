using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;

    [Header("Condition de victoire")]
    [SerializeField, Min(1f)] private float victoryTime = 60f;

    [Header("Écrans de fin")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    private float elapsedTime;
    private bool gameEnded;

    public GameConfig Config => gameConfig;
    public bool GameEnded => gameEnded;

    private void Awake()
    {
        if (gameConfig == null)
        {
            Debug.LogWarning("GameManager : aucun GameConfig assigné.", this);
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (gameEnded)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= victoryTime)
            Victory();
    }

    public void GameOver()
    {
        EndGame(gameOverPanel, "Game Over");
    }

    public void Victory()
    {
        EndGame(victoryPanel, "Victoire");
    }

    private void EndGame(GameObject panel, string message)
    {
        if (gameEnded)
            return;

        gameEnded = true;
        Debug.Log(message, this);

        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning($"GameManager : assigne le panneau « {message} » dans l’Inspector.", this);

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
        Debug.Log("Quitter le jeu fonctionne dans une version compilée, pas dans l’éditeur.");
    }
}