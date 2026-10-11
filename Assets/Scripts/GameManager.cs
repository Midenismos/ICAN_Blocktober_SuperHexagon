using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;

    public GameConfig Config => gameConfig;

    private void Awake()
    {
        if (gameConfig == null)
        {
            Debug.LogError(
                "GameConfig n'est pas assigné au GameManager.",
                this
            );
        }
    }
}