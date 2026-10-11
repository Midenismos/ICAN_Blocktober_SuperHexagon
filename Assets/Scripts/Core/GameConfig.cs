using UnityEngine;

public enum PrototypeMode
{
    Horizontal,
    Vertical,
    Circular
}

[CreateAssetMenu(
    fileName = "PrototypeConfig",
    menuName = "Prototype/Configuration de jeu")]
public class GameConfig : ScriptableObject
{
    [Header("Mode")]
    public PrototypeMode mode = PrototypeMode.Horizontal;

    [Header("Auto-scroll")]
    [Min(0f)]
    public float scrollSpeed = 5f;

    [Header("Personnage — modes horizontal et vertical")]
    [Min(0.1f)]
    public float moveSpeed = 5f;

    [Min(0.1f)]
    public float gravity = 25f;

    [Min(0.1f)]
    public float jumpForce = 9f;

    [Tooltip("Nombre d'unités par seconde pour changer de côté sur le cercle.")]
    [Min(1f)]
    public float circularDegreesPerSecond = 180f;

    [Header("Cercle")]
    [Min(0.1f)]
    public float circleRadius = 4f;

    [Header("Limites du niveau")]
    [Min(0.1f)]
    public float levelLimit = 20f;
}