using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player Stats")]
        public int maxHealth = 5;
        public int currentHealth;
        public int maxEnergy = 100;
        public int currentEnergy;

        [Header("Save Data")]
        private string saveFilePath;
        private GameSaveData currentSave;

        [Header("References")]
        public AudioManager audioManager;
        public UIManager uiManager;
        public AbilityManager abilityManager;

        private bool initialized;

        void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");

            InitializeManagers();
            LoadGame();
        }

        private void InitializeManagers()
        {
            if (initialized) return;

            // Find managers in current scene
            audioManager = FindObjectOfType<AudioManager>();
            uiManager = FindObjectOfType<UIManager>();
            abilityManager = FindObjectOfType<AbilityManager>();

            if (audioManager == null || uiManager == null || abilityManager == null)
            {
                Debug.LogWarning("One or more managers not found. Loading _Manager scene...");
                SceneManager.LoadSceneAsync("_Manager", LoadSceneMode.Additive);
            }

            initialized = true;
        }

        #region Player Stats

        public void ChangeHealth(int amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            uiManager?.UpdateHealthBar(currentHealth, maxHealth);

            if (currentHealth <= 0)
                OnPlayerDeath();
        }

        public void ChangeEnergy(int amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
            //uiManager?.UpdateEnergyBar(currentEnergy, maxEnergy);
        }

        private void OnPlayerDeath()
        {
            audioManager?.PlaySFX("PlayerDeath");
            uiManager?.ShowDeathScreen();
        }

        #endregion

        #region Save / Load

        public void SaveGame()
        {
            currentSave = new GameSaveData
            {
                health = currentHealth,
                energy = currentEnergy,
                unlockedAbilities = abilityManager?.GetUnlockedAbilities()
            };

            string json = JsonUtility.ToJson(currentSave, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game saved to {saveFilePath}");
        }

        public void LoadGame()
        {
            if (!File.Exists(saveFilePath))
            {
                Debug.Log("No save file found. Starting new game.");
                currentHealth = maxHealth;
                currentEnergy = maxEnergy;
                return;
            }

            string json = File.ReadAllText(saveFilePath);
            currentSave = JsonUtility.FromJson<GameSaveData>(json);

            currentHealth = currentSave.health;
            currentEnergy = currentSave.energy;
            abilityManager?.SetUnlockedAbilities(currentSave.unlockedAbilities);

            Debug.Log("Game loaded successfully.");
        }

        #endregion

        public void RespawnPlayer()
        {
            Debug.Log("Respawn");
        }

        public void SetCheckpoint(Transform respawnPoint)
        {
            Debug.Log("Set checkpoint");
        }
    }

    [System.Serializable]
    public class GameSaveData
    {
        public int health;
        public int energy;
        public string[] unlockedAbilities;
    }
}
