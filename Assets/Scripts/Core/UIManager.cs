using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Health & Energy")]
        [Tooltip("Fill-type Image that displays player health")]
        public Image healthBar;

        [Tooltip("Fill-type Image that displays player energy")]
        public Image energyBar;

        [Tooltip("Text showing current/max health (optional)")]
        public TextMeshProUGUI healthText;

        [Header("Ability Icons")]
        [Tooltip("Array of ability icon Images in the UI")]
        public Image[] abilityIcons;

        [Tooltip("Overlay images to show cooldown (will fill up as cooldown ends)")]
        public Image[] abilityCooldownOverlays;

        [Header("Pause Menu")]
        [Tooltip("Parent GameObject containing the pause menu")]
        public GameObject pauseMenuPanel;

        [Tooltip("Resume button in pause menu")]
        public Button resumeButton;

        [Tooltip("Quit button in pause menu")]
        public Button quitButton;

        [Header("Fade Transitions")]
        [Tooltip("CanvasGroup for fading the entire screen to black")]
        public CanvasGroup fadeGroup;

        [Tooltip("Image used for fade effect (should be black)")]
        public Image fadeImage;

        [Header("Damage Feedback")]
        [Tooltip("Red overlay that flashes when player takes damage")]
        public Image damageVignetteImage;

        [Header("Boss Health")]
        [Tooltip("Boss health bar (shown only during boss fights)")]
        public GameObject bossHealthBarPanel;

        [Tooltip("Fill-type Image for boss health")]
        public Image bossHealthBar;

        [Tooltip("Boss name text")]
        public TextMeshProUGUI bossNameText;

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

            InitializeUI();
        }

        void InitializeUI()
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (fadeGroup != null)
            {
                fadeGroup.alpha = 0f;
            }

            if (damageVignetteImage != null)
            {
                Color c = damageVignetteImage.color;
                c.a = 0f;
                damageVignetteImage.color = c;
            }

            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(false);
            }

            Debug.Log("UIManager initialized");
        }

        public void UpdateHealthBar()
        {
            if (healthBar != null && GameManager.Instance != null)
            {
                float healthPercent = (float)GameManager.Instance.currentHealth / GameManager.Instance.maxHealth;
                healthBar.fillAmount = healthPercent;

                if (healthText != null)
                {
                    healthText.text = $"{GameManager.Instance.currentHealth} / {GameManager.Instance.maxHealth}";
                }
            }
        }

        public void UpdateEnergyBar()
        {
            if (energyBar != null && GameManager.Instance != null)
            {
                float energyPercent = (float)GameManager.Instance.currentEnergy / GameManager.Instance.maxEnergy;
                energyBar.fillAmount = energyPercent;
            }
        }

        public void ShowDamageEffect()
        {
            if (damageVignetteImage != null)
            {
                StartCoroutine(DamageFlashCoroutine());
            }
        }

        private IEnumerator DamageFlashCoroutine()
        {
            Color c = damageVignetteImage.color;
            c.a = 0.5f;
            damageVignetteImage.color = c;

            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(0.5f, 0f, elapsed / duration);
                damageVignetteImage.color = c;
                yield return null;
            }

            c.a = 0f;
            damageVignetteImage.color = c;
        }

        public void UpdateAbilityIcons()
        {
            if (GameManager.Instance == null) return;

            List<string> unlockedAbilities = GameManager.Instance.unlockedAbilities;

            for (int i = 0; i < abilityIcons.Length; i++)
            {
                if (abilityIcons[i] != null)
                {
                    if (i < unlockedAbilities.Count)
                    {
                        abilityIcons[i].enabled = true;
                    }
                    else
                    {
                        abilityIcons[i].enabled = false;
                    }
                }
            }
        }

        public void UpdateAbilityCooldown(int abilityIndex, float cooldownPercent)
        {
            if (abilityIndex >= 0 && abilityIndex < abilityCooldownOverlays.Length)
            {
                if (abilityCooldownOverlays[abilityIndex] != null)
                {
                    abilityCooldownOverlays[abilityIndex].fillAmount = cooldownPercent;
                }
            }
        }

        public void ShowPauseMenu(bool show)
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(show);
            }
        }

        void OnResumeClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }

        void OnQuitClicked()
        {
            Debug.Log("Quit button clicked");
            Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void FadeOut(float duration)
        {
            StartCoroutine(FadeCoroutine(0f, 1f, duration));
        }

        public void FadeIn(float duration)
        {
            StartCoroutine(FadeCoroutine(1f, 0f, duration));
        }

        public void SetFade(float alpha)
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = alpha;
            }
        }

        private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
        {
            if (fadeGroup == null) yield break;

            float elapsed = 0f;
            fadeGroup.alpha = startAlpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            fadeGroup.alpha = endAlpha;
        }

        public void ShowBossHealthBar(string bossName)
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(true);
            }

            if (bossNameText != null)
            {
                bossNameText.text = bossName;
            }

            if (bossHealthBar != null)
            {
                bossHealthBar.fillAmount = 1f;
            }
        }

        public void UpdateBossHealthBar(float healthPercent)
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.fillAmount = healthPercent;
            }
        }

        public void HideBossHealthBar()
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(false);
            }
        }

        public void ShowMessage(string message, float duration = 3f)
        {
            Debug.Log($"UI Message: {message}");
            StartCoroutine(MessageCoroutine(message, duration));
        }

        private IEnumerator MessageCoroutine(string message, float duration)
        {
            yield return new WaitForSeconds(duration);
        }
    }
}