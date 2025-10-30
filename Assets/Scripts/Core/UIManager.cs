using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Game.Core
{
    [DefaultExecutionOrder(-80)]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image energyFill;
        [SerializeField] private Image damageOverlay;
        [SerializeField] private CanvasGroup fadeCanvas;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private TMP_Text messageText;

        [Header("Settings")]
        public float fadeDuration = 1f;
        public float damageFlashDuration = 0.2f;

        private bool isPaused;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvas != null)
                fadeCanvas.alpha = 0;

            if (pauseMenu != null)
                pauseMenu.SetActive(false);

            if (damageOverlay != null)
                damageOverlay.color = new Color(1, 0, 0, 0);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        #region Health & Energy

        public void UpdateHealthBar(int current, int max)
        {
            if (healthFill == null) return;
            float fill = (float)current / max;
            healthFill.fillAmount = fill;
        }

        public void UpdateEnergyBar(int current, int max)
        {
            if (energyFill == null) return;
            float fill = (float)current / max;
            energyFill.fillAmount = fill;
        }

        #endregion

        #region Damage Flash

        public void ShowDamageEffect()
        {
            if (damageOverlay == null) return;
            StopAllCoroutines();
            StartCoroutine(DamageFlashRoutine());
        }

        private System.Collections.IEnumerator DamageFlashRoutine()
        {
            damageOverlay.color = new Color(1, 0, 0, 0.4f);

            float t = 0;
            while (t < damageFlashDuration)
            {
                t += Time.deltaTime;
                damageOverlay.color = new Color(1, 0, 0, Mathf.Lerp(0.4f, 0, t / damageFlashDuration));
                yield return null;
            }

            damageOverlay.color = new Color(1, 0, 0, 0);
        }

        #endregion

        #region Fade

        public void FadeIn() => StartCoroutine(FadeRoutine(1, 0));
        public void FadeOut() => StartCoroutine(FadeRoutine(0, 1));

        private System.Collections.IEnumerator FadeRoutine(float from, float to)
        {
            if (fadeCanvas == null) yield break;

            fadeCanvas.blocksRaycasts = true;
            float elapsed = 0;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvas.alpha = to;
            fadeCanvas.blocksRaycasts = to > 0;
        }

        #endregion

        #region Pause Menu

        public void TogglePause()
        {
            isPaused = !isPaused;
            pauseMenu?.SetActive(isPaused);

            Time.timeScale = isPaused ? 0 : 1;
            AudioListener.pause = isPaused;
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1;
            AudioListener.pause = false;
            pauseMenu?.SetActive(false);
        }

        public void QuitToMainMenu()
        {
            ResumeGame();
            SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region Messages / Prompts

        public void ShowMessage(string text, float duration = 2f)
        {
            if (messageText == null) return;
            StopCoroutine(nameof(MessageRoutine));
            StartCoroutine(MessageRoutine(text, duration));
        }

        private System.Collections.IEnumerator MessageRoutine(string text, float duration)
        {
            messageText.text = text;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            messageText.gameObject.SetActive(false);
        }

        public void ShowDeathScreen()
        {
            FadeOut();
            ShowMessage("You Died", 2f);
        }

        #endregion
    }
}
