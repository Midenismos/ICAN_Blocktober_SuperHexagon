using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Serializable]
    private class LifeSegment
    {
        public GameObject visual;
        public Collider2D segmentCollider;
    }

    [Header("Vie")]
    [SerializeField, Min(1)] private int maxHealth = 4;
    [SerializeField, Min(1)] private int damagePerHit = 1;

    [Tooltip("Un élément par PV, dans l'ordre de perte. Le dernier élément est supprimé en premier.")]
    [SerializeField] private LifeSegment[] lifeSegments;

    [Header("Invulnérabilité")]
    [SerializeField, Min(0.01f)] private float invulnerabilityDuration = 1.25f;

    [Header("Effet visuel après un dégât")]
    [Tooltip("Couleur appliquée aux SpriteRenderers du joueur pendant l'invulnérabilité.")]
    [SerializeField] private Color invulnerabilityColor = Color.red;

    [Tooltip("SpriteRenderers à recolorer. Si la liste est vide, ceux du joueur et de ses enfants sont utilisés.")]
    [SerializeField] private SpriteRenderer[] playerRenderers;

    [Header("Détection des obstacles")]
    [SerializeField] private string obstacleTag = "Obstacle";
    [Tooltip("À activer si les obstacles utilisent Is Trigger.")]
    [SerializeField] private bool acceptTriggerDamage;

    [Header("À la mort")]
    [Tooltip("Ajoute ici le script de déplacement du joueur pour l'arrêter à sa mort. Ne pas ajouter PlayerHealth.")]
    [SerializeField] private MonoBehaviour[] componentsToDisableOnDeath;
    [SerializeField] private bool disableCollidersOnDeath = true;
    [SerializeField] private bool stopRigidbodyOnDeath = true;
    [SerializeField] private bool disableRigidbodySimulationOnDeath = true;

    [Header("Événements Unity")]
    public UnityEvent OnDamaged;
    public UnityEvent OnDeath;

    public event Action<int, int> OnHealthChanged;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => !isDead && Time.time < invulnerableUntil;

    private int currentHealth;
    private bool isDead;
    private float invulnerableUntil;
    private int lastDamageFrame = -1;

    private Rigidbody2D playerRigidbody;
    private GameManager gameManager;
    private Coroutine damageEffectCoroutine;
    private Color[] originalRendererColors;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damagePerHit = Mathf.Max(1, damagePerHit);

        playerRigidbody = GetComponent<Rigidbody2D>();

        if (gameManager == null)
        {
            GameObject managerObject = GameObject.Find("GameManager");
            if (managerObject != null)
                gameManager = managerObject.GetComponent<GameManager>();
        }

        if (playerRenderers == null || playerRenderers.Length == 0)
            playerRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        CacheOriginalColors();
        currentHealth = maxHealth;

        if (lifeSegments == null || lifeSegments.Length != maxHealth)
        {
            Debug.LogError(
                $"PlayerHealth : assigne exactement {maxHealth} segments de vie dans l'Inspector.",
                this
            );
        }

        if (lifeSegments != null)
        {
            for (int i = 0; i < lifeSegments.Length; i++)
                SetSegmentActive(i, i < currentHealth);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamageFromCollider(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamageFromCollider(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (acceptTriggerDamage)
            TryDamageFromCollider(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (acceptTriggerDamage)
            TryDamageFromCollider(other);
    }

    private void TryDamageFromCollider(Collider2D other)
    {
        if (other == null || isDead || !HasObstacleTagInParents(other.transform))
            return;

        TakeDamage(damagePerHit);
    }

    private bool HasObstacleTagInParents(Transform current)
    {
        while (current != null)
        {
            if (current.CompareTag(obstacleTag))
                return true;

            current = current.parent;
        }

        return false;
    }

    public bool TakeDamage(int amount)
    {
        if (isDead || amount <= 0 || IsInvulnerable)
            return false;

        // Évite plusieurs pertes de vie pendant la même frame.
        if (lastDamageFrame == Time.frameCount)
            return false;

        lastDamageFrame = Time.frameCount;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnerableUntil = Time.time + invulnerabilityDuration;

        StartDamageVisualEffect();

        // Désactive immédiatement les colliders des segments perdus,
        // mais laisse leurs visuels actifs pendant l'animation.
        for (int i = 0; i < lifeSegments.Length; i++)
        {
            bool stillAlive = i < currentHealth;

            if (stillAlive)
            {
                SetSegmentActive(i, true);
            }
            else if (i < previousHealth)
            {
                StartCoroutine(ShrinkAndHideSegment(i));
            }
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();

        if (currentHealth == 0)
            Die();

        return true;
    }

    private IEnumerator ShrinkAndHideSegment(int index)
    {
        if (lifeSegments == null || index < 0 || index >= lifeSegments.Length)
            yield break;

        LifeSegment segment = lifeSegments[index];

        if (segment == null)
            yield break;

        if (segment.segmentCollider != null)
            segment.segmentCollider.enabled = false;

        if (segment.visual == null)
            yield break;

        Transform visualTransform = segment.visual.transform;
        Vector3 startScale = visualTransform.localScale;
        float elapsed = 0f;

        while (elapsed < invulnerabilityDuration && visualTransform != null)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / invulnerabilityDuration);
            visualTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        if (segment.visual != null)
        {
            visualTransform.localScale = Vector3.zero;
            segment.visual.SetActive(false);
        }
    }

    private void StartDamageVisualEffect()
    {
        if (damageEffectCoroutine != null)
            StopCoroutine(damageEffectCoroutine);

        damageEffectCoroutine = StartCoroutine(DamageVisualEffect());
    }

    private IEnumerator DamageVisualEffect()
    {
        SetRendererColors(invulnerabilityColor);
        yield return new WaitForSeconds(invulnerabilityDuration);
        RestoreRendererColors();
        damageEffectCoroutine = null;
    }

    private void CacheOriginalColors()
    {
        originalRendererColors = new Color[playerRenderers.Length];

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
                originalRendererColors[i] = playerRenderers[i].color;
        }
    }

    private void SetRendererColors(Color color)
    {
        if (playerRenderers == null)
            return;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
                playerRenderers[i].color = color;
        }
    }

    private void RestoreRendererColors()
    {
        if (playerRenderers == null || originalRendererColors == null)
            return;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
                playerRenderers[i].color = originalRendererColors[i];
        }
    }

    private void SetSegmentActive(int index, bool active)
    {
        if (lifeSegments == null || index < 0 || index >= lifeSegments.Length)
            return;

        LifeSegment segment = lifeSegments[index];

        if (segment == null)
            return;

        if (segment.visual != null)
            segment.visual.SetActive(active);

        if (segment.segmentCollider != null)
            segment.segmentCollider.enabled = active;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (stopRigidbodyOnDeath && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        if (disableCollidersOnDeath && lifeSegments != null)
        {
            foreach (LifeSegment segment in lifeSegments)
            {
                if (segment != null && segment.segmentCollider != null)
                    segment.segmentCollider.enabled = false;
            }
        }

        if (disableRigidbodySimulationOnDeath && playerRigidbody != null)
            playerRigidbody.simulated = false;

        if (componentsToDisableOnDeath != null)
        {
            foreach (MonoBehaviour componentToDisable in componentsToDisableOnDeath)
            {
                if (componentToDisable != null && componentToDisable != this)
                    componentToDisable.enabled = false;
            }
        }

        OnDeath?.Invoke();

        if (gameManager != null)
            gameManager.GameOver();
        else
            Debug.LogWarning("PlayerHealth : aucun GameManager trouvé dans la scène.", this);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damagePerHit = Mathf.Max(1, damagePerHit);
    }
}