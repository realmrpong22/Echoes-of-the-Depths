using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    [DefaultExecutionOrder(-70)]
    public class AbilityManager : MonoBehaviour
    {
        public static AbilityManager Instance { get; private set; }

        [Header("Ability Settings")]
        [Tooltip("Cooldown times per ability (seconds)")]
        public List<AbilityData> abilities = new List<AbilityData>();

        private Dictionary<string, AbilityData> abilityLookup = new();
        private Dictionary<string, float> cooldownTimers = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize lookup dictionary
            foreach (var ability in abilities)
            {
                if (!abilityLookup.ContainsKey(ability.id))
                    abilityLookup.Add(ability.id, ability);

                if (!cooldownTimers.ContainsKey(ability.id))
                    cooldownTimers.Add(ability.id, 0f);
            }
        }

        void Update()
        {
            // Reduce cooldown timers over time
            var keys = new List<string>(cooldownTimers.Keys);
            foreach (var key in keys)
            {
                if (cooldownTimers[key] > 0)
                    cooldownTimers[key] -= Time.deltaTime;
            }
        }

        #region Public Methods

        /// <summary>
        /// Try to activate an ability if unlocked and off cooldown.
        /// Returns true if successful.
        /// </summary>
        public bool TryUseAbility(string id)
        {
            if (!abilityLookup.ContainsKey(id))
            {
                Debug.LogWarning($"Ability '{id}' not found!");
                return false;
            }

            var ability = abilityLookup[id];
            if (!ability.unlocked)
            {
                Debug.Log($"Ability '{id}' is locked.");
                return false;
            }

            if (cooldownTimers[id] > 0)
            {
                Debug.Log($"Ability '{id}' is on cooldown for {cooldownTimers[id]:F1}s.");
                return false;
            }

            if (GameManager.Instance.currentEnergy < ability.energyCost)
            {
                UIManager.Instance?.ShowMessage("Not enough energy!");
                return false;
            }

            // Deduct energy and trigger effect
            GameManager.Instance.ChangeEnergy(-ability.energyCost);
            cooldownTimers[id] = ability.cooldown;

            // Trigger the ability’s effect (if hooked)
            ability.TriggerAbility();

            // Visual feedback
            UIManager.Instance?.ShowMessage($"{ability.displayName} used!");
            AudioManager.Instance?.PlaySFX(ability.sfxName);

            return true;
        }

        public void UnlockAbility(string id)
        {
            if (!abilityLookup.ContainsKey(id)) return;

            abilityLookup[id].unlocked = true;
            UIManager.Instance?.ShowMessage($"{abilityLookup[id].displayName} unlocked!");
            AudioManager.Instance?.PlaySFX("AbilityUnlock");

            GameManager.Instance.SaveGame();
        }

        public string[] GetUnlockedAbilities()
        {
            List<string> unlocked = new();
            foreach (var ability in abilityLookup.Values)
            {
                if (ability.unlocked)
                    unlocked.Add(ability.id);
            }
            return unlocked.ToArray();
        }

        public void SetUnlockedAbilities(string[] unlockedIds)
        {
            foreach (var id in unlockedIds)
            {
                if (abilityLookup.ContainsKey(id))
                    abilityLookup[id].unlocked = true;
            }
        }

        #endregion
    }

    [System.Serializable]
    public class AbilityData
    {
        [Header("Base Info")]
        public string id;
        public string displayName;
        public bool unlocked;

        [Header("Gameplay")]
        public float cooldown = 1f;
        public int energyCost = 10;

        [Header("Visuals & Audio")]
        public string sfxName = "AbilityUse";

        [Header("Ability Reference")]
        [Tooltip("Optional script or object to trigger when used.")]
        public GameObject abilityObject;

        public void TriggerAbility()
        {
            if (abilityObject != null)
            {
                var activatable = abilityObject.GetComponent<IActivatableAbility>();
                if (activatable != null)
                    activatable.Activate();
            }
        }
    }

    /// <summary>
    /// Optional interface for custom ability scripts.
    /// </summary>
    public interface IActivatableAbility
    {
        void Activate();
    }
}
