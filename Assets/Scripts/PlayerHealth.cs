using System;
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

    [Header("Détection des obstacles")]
    [SerializeField] private string obstacleTag = "Obstacle";
    [Tooltip("À activer si les obstacles utilisent Is Trigger.")]
    [SerializeField] private bool acceptTriggerDamage;

    [Header("À la mort")]
    [Tooltip("Ajoute ici le script de déplacement du joueur pour l'arrêter à sa mort.")]
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

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damagePerHit = Mathf.Max(1, damagePerHit);

        playerRigidbody = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (lifeSegments == null || lifeSegments.Length != maxHealth)
        {
            Debug.LogError(
                $"PlayerHealth : assigne exactement {maxHealth} segments de vie dans l'Inspector.",
                this
            );
        }

        // Au démarrage, active les segments correspondant aux PV disponibles.
        if (lifeSegments != null)
        {
            for (int i = 0; i < lifeSegments.Length; i++)
            {
                bool shouldBeActive = i < currentHealth;
                SetSegmentActive(i, shouldBeActive);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"PlayerHealth reçoit une collision avec : {collision.gameObject.name}", this);
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

        // Évite de perdre plusieurs PV pendant la même frame.
        if (lastDamageFrame == Time.frameCount)
            return false;

        lastDamageFrame = Time.frameCount;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnerableUntil = Time.time + invulnerabilityDuration;

        RemoveLostSegments();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();

        if (currentHealth == 0)
            Die();

        return true;
    }

    private void RemoveLostSegments()
    {
        if (lifeSegments == null)
            return;

        // Les segments sont renseignés dans l'ordre de perte :
        // le dernier segment assigné est supprimé en premier.
        for (int i = 0; i < lifeSegments.Length; i++)
        {
            bool shouldBeActive = i < currentHealth;
            SetSegmentActive(i, shouldBeActive);
        }
    }

    private void SetSegmentActive(int index, bool active)
    {
        if (lifeSegments == null || index < 0 || index >= lifeSegments.Length)
            return;

        LifeSegment segment = lifeSegments[index];

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
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damagePerHit = Mathf.Max(1, damagePerHit);
    }


}