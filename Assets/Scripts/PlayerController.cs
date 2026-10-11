using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform circleCenter;

    [Header("Déplacement circulaire")]
    [SerializeField, Min(0.1f)] private float radius = 3f;
    [SerializeField, Min(0f)] private float rotationSpeed = 180f;

    [Header("Contrôles")]
    [SerializeField, Range(0f, 1f)] private float inputDeadzone = 0.2f;

    private Rigidbody2D rb;
    private float angle;
    private float inputDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (circleCenter == null)
        {
            Debug.LogError(
                "PlayerController : assigne le centre de l'arène au champ Circle Center.",
                this
            );
            enabled = false;
            return;
        }

        Vector2 offset = (Vector2)transform.position - (Vector2)circleCenter.position;

        angle = offset.sqrMagnitude > 0.001f
            ? Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg
            : 0f;
    }

    private void Update()
    {
        inputDirection = ReadInput();
        angle += inputDirection * rotationSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (circleCenter == null)
            return;

        float radians = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;

        rb.MovePosition((Vector2)circleCenter.position + offset);
    }

    private float ReadInput()
    {
        float direction = 0f;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // Gauche/A/Q : sens positif ; droite/D : sens négatif.
            if (keyboard.leftArrowKey.isPressed ||
                keyboard.aKey.isPressed ||
                keyboard.qKey.isPressed)
            {
                direction += 1f;
            }

            if (keyboard.rightArrowKey.isPressed ||
                keyboard.dKey.isPressed)
            {
                direction -= 1f;
            }
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            float stick = gamepad.leftStick.ReadValue().x;

            if (Mathf.Abs(stick) > inputDeadzone)
                direction -= Mathf.Sign(stick);
        }

        return Mathf.Clamp(direction, -1f, 1f);
    }
}