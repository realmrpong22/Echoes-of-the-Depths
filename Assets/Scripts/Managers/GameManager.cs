using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isPaused = false;

    [Header("Player Progress")]
    public List<string> unlockedAbilities = new List<string>();
    public Dictionary<string, bool> bossesDefeated = new Dictionary<string, bool>();
    public Dictionary<string, bool> puzzlesCompleted = new Dictionary<string, bool>();

    [Header("Respawn")]
    public Transform respawnPoint;
    public Vector3 lastCheckpointPosition;

    [Header("Player Stats")]
    public int currentHealth = 100;
    public int maxHealth = 100;
    public int currentEnergy = 100;
    public int maxEnergy = 100;

    [Header("Current Region")]
    public string currentRegion = "Region_01";
    public int currentRegionIndex = 0;

    void Awake()
    {
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

        InitializeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void InitializeGame()
    {
        unlockedAbilities.Add("Dash");
        unlockedAbilities.Add("DoubleJump");
        unlockedAbilities.Add("WallJump");

        bossesDefeated.Add("Boss1", false);
        bossesDefeated.Add("Boss2", false);
        bossesDefeated.Add("Boss3", false);

        puzzlesCompleted.Add("Region_01_Puzzle", false);
        puzzlesCompleted.Add("Region_02_Puzzle", false);
        puzzlesCompleted.Add("Region_03_Puzzle", false);

        Debug.Log("GameManager initialized successfully");
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(isPaused);
        }

        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    public void UnlockAbility(string abilityName)
    {
        if (!unlockedAbilities.Contains(abilityName))
        {
            unlockedAbilities.Add(abilityName);
            Debug.Log($"Ability unlocked: {abilityName}");

            // Notify UI to update ability icons
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateAbilityIcons();
            }

            SaveGame();
        }
    }

    public bool HasAbility(string abilityName)
    {
        return unlockedAbilities.Contains(abilityName);
    }

    public void DefeatBoss(string bossName)
    {
        if (bossesDefeated.ContainsKey(bossName))
        {
            bossesDefeated[bossName] = true;
            Debug.Log($"Boss defeated: {bossName}");
            SaveGame();
        }
    }

    public bool IsBossDefeated(string bossName)
    {
        return bossesDefeated.ContainsKey(bossName) && bossesDefeated[bossName];
    }

    public void CompletePuzzle(string puzzleName)
    {
        if (puzzlesCompleted.ContainsKey(puzzleName))
        {
            puzzlesCompleted[puzzleName] = true;
            Debug.Log($"Puzzle completed: {puzzleName}");
            SaveGame();
        }
    }

    public bool IsPuzzleCompleted(string puzzleName)
    {
        return puzzlesCompleted.ContainsKey(puzzleName) && puzzlesCompleted[puzzleName];
    }

    public void SetCheckpoint(Transform checkpointTransform)
    {
        respawnPoint = checkpointTransform;
        lastCheckpointPosition = checkpointTransform.position;

        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthBar();
            UIManager.Instance.UpdateEnergyBar();
        }

        Debug.Log($"Checkpoint set at: {lastCheckpointPosition}");
        SaveGame();
    }

    public void RespawnPlayer()
    {
        // Find the player and move them to checkpoint
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && respawnPoint != null)
        {
            player.transform.position = lastCheckpointPosition;

            // Restore health and energy
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;

            // Update UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealthBar();
                UIManager.Instance.UpdateEnergyBar();
            }

            Debug.Log("Player respawned at checkpoint");
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            playerPosition = lastCheckpointPosition,
            currentHealth = currentHealth,
            maxHealth = maxHealth,
            currentEnergy = currentEnergy,
            maxEnergy = maxEnergy,
            unlockedAbilities = unlockedAbilities,
            bossesDefeated = bossesDefeated,
            puzzlesCompleted = puzzlesCompleted,
            currentRegion = currentRegion
        };

        string json = JsonUtility.ToJson(data, true); // true = pretty print
        System.IO.File.WriteAllText(Application.persistentDataPath + "/savegame.json", json);

        Debug.Log("Game saved to: " + Application.persistentDataPath + "/savegame.json");
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/savegame.json";

        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Restore all saved data
            lastCheckpointPosition = data.playerPosition;
            currentHealth = data.currentHealth;
            maxHealth = data.maxHealth;
            currentEnergy = data.currentEnergy;
            maxEnergy = data.maxEnergy;
            unlockedAbilities = data.unlockedAbilities;
            bossesDefeated = data.bossesDefeated;
            puzzlesCompleted = data.puzzlesCompleted;
            currentRegion = data.currentRegion;

            // Move player to saved position
            RespawnPlayer();

            Debug.Log("Game loaded successfully");
        }
        else
        {
            Debug.LogWarning("No save file found");
        }
    }
}

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public int currentHealth;
    public int maxHealth;
    public int currentEnergy;
    public int maxEnergy;
    public List<string> unlockedAbilities;
    public Dictionary<string, bool> bossesDefeated;
    public Dictionary<string, bool> puzzlesCompleted;
    public string currentRegion;
}