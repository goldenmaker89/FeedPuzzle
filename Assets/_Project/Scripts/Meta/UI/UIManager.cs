using UnityEngine;
using UnityEngine.UI;
using Core;

namespace Meta.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private GameObject hud;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button victoryRestartButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
            
            // Subscribe to buttons
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseButtonClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeButtonClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartButtonClicked);
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
            if (victoryRestartButton != null) victoryRestartButton.onClick.AddListener(OnRestartButtonClicked);

            // Initial state
            ShowHUD(true);
            ShowPauseMenu(false);
            ShowGameOver(false);
            ShowVictory(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
            
            // Unsubscribe
            if (pauseButton != null) pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
            if (victoryRestartButton != null) victoryRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    ShowHUD(true);
                    ShowPauseMenu(false);
                    break;
                case GameState.Paused:
                    ShowHUD(true); // Or false if pause covers everything
                    ShowPauseMenu(true);
                    break;
                case GameState.GameOver:
                    ShowHUD(false);
                    ShowGameOver(true);
                    break;
                case GameState.Victory:
                    ShowHUD(false);
                    ShowVictory(true);
                    break;
            }
        }

        public void ShowHUD(bool show)
        {
            if (hud != null) hud.SetActive(show);
        }

        public void ShowPauseMenu(bool show)
        {
            if (pauseMenu != null) pauseMenu.SetActive(show);
        }

        public void ShowGameOver(bool show)
        {
            if (gameOverScreen != null) gameOverScreen.SetActive(show);
        }

        public void ShowVictory(bool show)
        {
            if (victoryScreen != null) victoryScreen.SetActive(show);
        }

        public void OnPauseButtonClicked()
        {
            GameManager.Instance.TogglePause();
        }

        public void OnResumeButtonClicked()
        {
            GameManager.Instance.SetState(GameState.Playing);
        }

        public void OnRestartButtonClicked()
        {
            // Reload scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
