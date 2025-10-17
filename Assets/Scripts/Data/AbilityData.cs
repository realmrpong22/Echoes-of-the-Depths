using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Data/Ability")]
public class AbilityData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Internal name used in code (e.g., 'Dash', 'DoubleJump')")]
    public string abilityName;

    [Tooltip("Display name shown to player")]
    public string displayName;

    [TextArea(3, 5)]
    [Tooltip("Description shown in UI")]
    public string description;

    [Header("Visuals")]
    [Tooltip("Icon displayed in UI")]
    public Sprite icon;

    [Tooltip("Color tint for ability effects")]
    public Color abilityColor = Color.white;

    [Header("Gameplay")]
    [Tooltip("Type of ability - determines behavior")]
    public AbilityType type;

    [Tooltip("Cooldown in seconds (0 = no cooldown)")]
    public float cooldown = 0f;

    [Tooltip("Energy cost to use ability (0 = free)")]
    public int energyCost = 0;

    [Tooltip("Is this ability unlocked from the start?")]
    public bool unlockedAtStart = false;

    [Header("Unlock Requirements")]
    [Tooltip("Which boss must be defeated to unlock this")]
    public string requiredBoss = "";

    [Tooltip("Custom unlock message")]
    public string unlockMessage = "New Ability Unlocked!";

    [Header("Audio")]
    [Tooltip("Sound effect name to play when using ability")]
    public string sfxName = "";
}

public enum AbilityType
{
    Movement,
    Combat,
    Utility,
    Passive
}