using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHealth))]
public class HealthQuadrantsView : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Sprite cellSprite;

    [Header("Apparence")]
    [SerializeField, Min(0.01f)] private float squareSize = 1f;
    [SerializeField, Min(0f)] private float gap = 0.025f;
    [SerializeField] private Color aliveColor = Color.white;
    [SerializeField] private Color lostColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private int sortingOrder = 10;

    private const string CellPrefix = "HealthCell_";
    private int lastMaxHealth = -1;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        if (health == null || cellSprite == null)
        {
            Debug.LogError(
                "HealthQuadrantsView : assigne PlayerHealth et un sprite carré dans l'Inspector.",
                this
            );
            return;
        }

        RebuildCells();
        RefreshCells();
    }

    private void HandleHealthChanged(int current, int maximum)
    {
        if (maximum != lastMaxHealth)
            RebuildCells();

        RefreshCells();
    }

    private void RebuildCells()
    {
        if (health == null || cellSprite == null)
            return;

        ClearGeneratedCells();

        int count = health.MaxHealth;
        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);

        float cellWidth = squareSize / columns;
        float cellHeight = squareSize / rows;
        Vector2 spriteSize = cellSprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            Debug.LogError("HealthQuadrantsView : le sprite n'a pas de dimensions valides.", this);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            int column = i % columns;
            int row = i / columns;

            GameObject cell = new GameObject($"{CellPrefix}{i:00}");
            cell.transform.SetParent(transform, false);

            cell.transform.localPosition = new Vector3(
                -squareSize * 0.5f + cellWidth * (column + 0.5f),
                squareSize * 0.5f - cellHeight * (row + 0.5f),
                0f
            );

            cell.transform.localScale = new Vector3(
                Mathf.Max(0.001f, cellWidth - gap) / spriteSize.x,
                Mathf.Max(0.001f, cellHeight - gap) / spriteSize.y,
                1f
            );

            SpriteRenderer renderer = cell.AddComponent<SpriteRenderer>();
            renderer.sprite = cellSprite;
            renderer.color = aliveColor;
            renderer.sortingOrder = sortingOrder;
        }

        lastMaxHealth = count;
    }

    private void RefreshCells()
    {
        if (health == null)
            return;

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (!child.name.StartsWith(CellPrefix))
                continue;

            string number = child.name.Substring(CellPrefix.Length);
            if (!int.TryParse(number, out int index))
                continue;

            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.color = index < health.CurrentHealth ? aliveColor : lostColor;
        }
    }

    private void ClearGeneratedCells()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (!child.name.StartsWith(CellPrefix))
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}