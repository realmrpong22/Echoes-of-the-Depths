using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class AbilityManager : MonoBehaviour
    {
        public static AbilityManager Instance { get; private set; }

        [Header("Ability Database")]
        [Tooltip("All abilities in the game - drag AbilityData assets here")]
        public List<AbilityData> allAbilities = new List<AbilityData>();

        [Header("Runtime State")]
        [Tooltip("Currently unlocked abilities (updated at runtime)")]
        public List<AbilityData> unlockedAbilities = new List<AbilityData>();

        // Dictionary for fast ability lookup by name
        private Dictionary<string, AbilityData> abilityDictionary = new Dictionary<string, AbilityData>();

        // Cooldown tracking
        private Dictionary<string, float> abilityCooldowns = new Dictionary<string, float>();

        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeAbilities();
        }

        void Update()
        {
            // Update cooldowns
            UpdateCooldowns();
        }

        void InitializeAbilities()
        {
            // Build dictionary from list
            foreach (AbilityData ability in allAbilities)
            {
                if (!abilityDictionary.ContainsKey(ability.abilityName))
                {
                    abilityDictionary.Add(ability.abilityName, ability);
                    abilityCooldowns.Add(ability.abilityName, 0f);
                }
            }

            // Unlock starting abilities
            foreach (AbilityData ability in allAbilities)
            {
                if (ability.unlockedAtStart)
                {
                    UnlockAbility(ability.abilityName, false);
                }
            }

            Debug.Log($"AbilityManager initialized with {allAbilities.Count} abilities");
        }

        void UpdateCooldowns()
        {
            List<string> keys = new List<string>(abilityCooldowns.Keys);
            foreach (string abilityName in keys)
            {
                if (abilityCooldowns[abilityName] > 0f)
                {
                    abilityCooldowns[abilityName] -= Time.deltaTime;

                    // Update UI cooldown display
                    if (UIManager.Instance != null)
                    {
                        AbilityData ability = GetAbilityData(abilityName);
                        if (ability != null && ability.cooldown > 0f)
                        {
                            int index = unlockedAbilities.IndexOf(ability);
                            if (index >= 0)
                            {
                                float percent = abilityCooldowns[abilityName] / ability.cooldown;
                                UIManager.Instance.UpdateAbilityCooldown(index, percent);
                            }
                        }
                    }
                }
            }
        }

        public void UnlockAbility(string abilityName, bool showMessage = true)
        {
            AbilityData ability = GetAbilityData(abilityName);

            if (ability == null)
            {
                Debug.LogWarning($"Ability not found: {abilityName}");
                return;
            }

            if (IsAbilityUnlocked(abilityName))
            {
                Debug.Log($"Ability already unlocked: {abilityName}");
                return;
            }

            // Add to unlocked list
            unlockedAbilities.Add(ability);

            // Also add to GameManager's list
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockAbility(abilityName);
            }

            // Show unlock message
            if (showMessage)
            {
                UIManager.Instance?.ShowMessage($"{ability.unlockMessage}\n{ability.displayName}", 3f);
                AudioManager.Instance?.PlaySFX("AbilityUnlock");
            }

            Debug.Log($"Ability unlocked: {ability.displayName}");
        }

        public bool IsAbilityUnlocked(string abilityName)
        {
            return unlockedAbilities.Exists(a => a.abilityName == abilityName);
        }

        public bool TryUseAbility(string abilityName)
        {
            AbilityData ability = GetAbilityData(abilityName);

            if (ability == null) return false;
            if (!IsAbilityUnlocked(abilityName)) return false;
            if (!IsAbilityReady(abilityName)) return false;

            // Check energy cost
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.currentEnergy < ability.energyCost)
                {
                    Debug.Log($"Not enough energy for {abilityName}");
                    return false;
                }

                // Consume energy
                GameManager.Instance.currentEnergy -= ability.energyCost;
                UIManager.Instance?.UpdateEnergyBar();
            }

            // Start cooldown
            if (ability.cooldown > 0f)
            {
                abilityCooldowns[abilityName] = ability.cooldown;
            }

            // Play sound effect
            if (!string.IsNullOrEmpty(ability.sfxName))
            {
                AudioManager.Instance?.PlaySFX(ability.sfxName);
            }

            return true;
        }

        public bool IsAbilityReady(string abilityName)
        {
            if (!abilityCooldowns.ContainsKey(abilityName)) return false;
            return abilityCooldowns[abilityName] <= 0f;
        }

        public float GetAbilityCooldown(string abilityName)
        {
            if (abilityCooldowns.ContainsKey(abilityName))
            {
                return Mathf.Max(0f, abilityCooldowns[abilityName]);
            }
            return 0f;
        }

        public AbilityData GetAbilityData(string abilityName)
        {
            if (abilityDictionary.ContainsKey(abilityName))
            {
                return abilityDictionary[abilityName];
            }
            return null;
        }

        public List<AbilityData> GetAbilitiesByType(AbilityType type)
        {
            return unlockedAbilities.FindAll(a => a.type == type);
        }

        public void LockAbility(string abilityName)
        {
            AbilityData ability = unlockedAbilities.Find(a => a.abilityName == abilityName);
            if (ability != null)
            {
                unlockedAbilities.Remove(ability);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.unlockedAbilities.Remove(abilityName);
                }

                Debug.Log($"Ability locked: {abilityName}");
            }
        }

        public void ResetAllCooldowns()
        {
            List<string> keys = new List<string>(abilityCooldowns.Keys);
            foreach (string key in keys)
            {
                abilityCooldowns[key] = 0f;
            }
        }

        [ContextMenu("Unlock All Abilities")]
        public void UnlockAllAbilities()
        {
            foreach (AbilityData ability in allAbilities)
            {
                UnlockAbility(ability.abilityName, false);
            }
            Debug.Log("All abilities unlocked!");
        }

        [ContextMenu("Lock All Abilities")]
        public void LockAllAbilities()
        {
            unlockedAbilities.Clear();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.unlockedAbilities.Clear();
            }
            Debug.Log("All abilities locked!");
        }
    }
}