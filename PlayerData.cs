using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Player Data", order = 1000)]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float baseSpeed = 6.5f;
    public float accelerationTime = 0.1f;
    public float deaccelerationTime = 0.1f;
    public float airAccelerationModifier = 0.5f;

    [Header("Jumping")]
    public float maxJumpHeight = 3.5f;
    public float timeToJumpApex = 0.75f;
    public float fallGravityMultiplier = 1.75f;
    public float coyoteTime = 0.1f;

    [Header("Combat")]
    public int baseHitPoints = 6;
    public float damageInvincibilityTime = 1f;
}
