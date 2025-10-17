using UnityEngine;

public class AbilityTester : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("=== Unlocking Ranged Pulse ===");
            AbilityManager.Instance.UnlockAbility("RangedPulse");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("=== Unlocking Super Dash ===");
            AbilityManager.Instance.UnlockAbility("SuperDash");
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("=== Unlocking ALL Abilities ===");
            AbilityManager.Instance.UnlockAllAbilities();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TestAllAbilities();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("--- Left Shift Pressed ---");

            bool hasAbility = GameManager.Instance.HasAbility("Dash");
            bool isUnlocked = AbilityManager.Instance.IsAbilityUnlocked("Dash");
            bool isReady = AbilityManager.Instance.IsAbilityReady("Dash");

            Debug.Log($"GameManager.HasAbility('Dash'): {hasAbility}");
            Debug.Log($"AbilityManager.IsAbilityUnlocked('Dash'): {isUnlocked}");
            Debug.Log($"AbilityManager.IsAbilityReady('Dash'): {isReady}");

            if (AbilityManager.Instance.TryUseAbility("Dash"))
            {
                Debug.Log("✓ DASH ABILITY USED SUCCESSFULLY!");
            }
            else
            {
                Debug.LogWarning("✗ Dash ability failed - check cooldown or unlock status");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("--- Space Pressed ---");

            bool hasDoubleJump = GameManager.Instance.HasAbility("DoubleJump");
            bool hasWallJump = GameManager.Instance.HasAbility("WallJump");

            Debug.Log($"Has DoubleJump: {hasDoubleJump}");
            Debug.Log($"Has WallJump: {hasWallJump}");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ListUnlockedAbilities();
        }
    }

    void TestAllAbilities()
    {
        Debug.Log("========== ABILITY STATUS TEST ==========");

        string[] abilityNames = { "Dash", "DoubleJump", "WallJump", "RangedPulse", "SuperDash" };

        foreach (string abilityName in abilityNames)
        {
            bool gmHas = GameManager.Instance.HasAbility(abilityName);
            bool amUnlocked = AbilityManager.Instance.IsAbilityUnlocked(abilityName);
            bool amReady = AbilityManager.Instance.IsAbilityReady(abilityName);
            float cooldown = AbilityManager.Instance.GetAbilityCooldown(abilityName);

            Debug.Log($"{abilityName}:");
            Debug.Log($"  - GameManager: {(gmHas ? "✓" : "✗")}");
            Debug.Log($"  - AbilityManager: {(amUnlocked ? "✓" : "✗")}");
            Debug.Log($"  - Ready: {(amReady ? "✓" : "✗")} (Cooldown: {cooldown:F2}s)");
        }

        Debug.Log("=========================================");
    }

    void ListUnlockedAbilities()
    {
        Debug.Log("========== UNLOCKED ABILITIES ==========");

        if (AbilityManager.Instance.unlockedAbilities.Count == 0)
        {
            Debug.Log("No abilities unlocked!");
        }
        else
        {
            foreach (AbilityData ability in AbilityManager.Instance.unlockedAbilities)
            {
                Debug.Log($"✓ {ability.displayName} ({ability.abilityName})");
            }
        }

        Debug.Log("=========================================");
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(10, 10, 10, 10);

        string instructions =
            "=== ABILITY TESTER ===\n" +
            "1 - Unlock Ranged Pulse\n" +
            "2 - Unlock Super Dash\n" +
            "0 - Unlock ALL Abilities\n" +
            "T - Test All Ability Status\n" +
            "L - List Unlocked Abilities\n" +
            "Shift - Test Dash (watch Console)\n" +
            "Space - Test Jump (watch Console)";

        GUI.Label(new Rect(10, 10, 300, 200), instructions, style);

        int unlockedCount = AbilityManager.Instance.unlockedAbilities.Count;
        GUI.Label(new Rect(10, 220, 300, 30), $"Unlocked: {unlockedCount}/5", style);
    }
}