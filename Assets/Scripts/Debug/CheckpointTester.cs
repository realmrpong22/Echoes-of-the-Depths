using UnityEngine;

public class CheckpointTester : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("How much damage to deal when pressing K")]
    public int testDamage = 30;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            DamagePlayer();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            KillPlayer();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnPlayer();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            HealPlayer();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }
    }

    void DamagePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                Debug.Log($"Dealing {testDamage} damage to player");
                pc.TakeDamage(testDamage);
            }
        }
    }

    void KillPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                Debug.Log("Killing player instantly");
                pc.TakeDamage(9999);
            }
        }
    }

    void RespawnPlayer()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("Manual respawn triggered");
            GameManager.Instance.RespawnPlayer();
        }
    }

    void HealPlayer()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHealth = GameManager.Instance.maxHealth;
            UIManager.Instance?.UpdateHealthBar();
            Debug.Log("Player healed to full health");
        }
    }

    void SaveGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
            Debug.Log("Game saved manually");
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;
        style.padding = new RectOffset(10, 10, 10, 10);

        string instructions =
            "=== CHECKPOINT TESTER ===\n" +
            "K - Damage Player (30 HP)\n" +
            "P - Kill Player (test respawn)\n" +
            "R - Manual Respawn\n" +
            "H - Heal to Full\n" +
            "S - Save Game\n\n" +
            "Walk into checkpoint to activate it!";

        GUI.Label(new Rect(10, Screen.height - 180, 300, 200), instructions, style);

        if (GameManager.Instance != null)
        {
            string healthInfo = $"HP: {GameManager.Instance.currentHealth}/{GameManager.Instance.maxHealth}";
            GUI.Label(new Rect(10, Screen.height - 200, 300, 30), healthInfo, style);
        }
    }
}