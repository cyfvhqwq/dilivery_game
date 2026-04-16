using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace CourierCity.UI
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName) => StartCoroutine(FadeAndLoad(sceneName));
        public void ReloadCurrent() => LoadScene(SceneManager.GetActiveScene().name);
        public void LoadMainMenu() => LoadScene("MainMenu");

        private IEnumerator FadeAndLoad(string sceneName)
        {
            yield return StartCoroutine(Fade(1f));
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return StartCoroutine(Fade(0f));
        }

        private IEnumerator Fade(float target)
        {
            if (!fadeGroup) yield break;
            float start = fadeGroup.alpha;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
            fadeGroup.alpha = target;
        }
    }

    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text deliveriesText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Core.GameManager gameManager;

        private void Start()
        {
            gameObject.SetActive(false);
            if (gameManager) gameManager.OnGameOver.AddListener(Show);
            if (retryButton) retryButton.onClick.AddListener(() => SceneLoader.Instance?.ReloadCurrent());
            if (menuButton) menuButton.onClick.AddListener(() => SceneLoader.Instance?.LoadMainMenu());
        }

        private void Show()
        {
            gameObject.SetActive(true);
            if (finalScoreText) finalScoreText.text = $"{gameManager.Score:N0}";
            Time.timeScale = 0f;
        }

        private void OnDestroy() => Time.timeScale = 1f;
    }
}
