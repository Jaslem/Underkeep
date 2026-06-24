// PauseManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu Panel")]
    public GameObject pauseMenuPanel;

    [Header("Controls Overlay")]
    public GameObject controlsOverlay;

    [Header("Button Images")]
    public Image resumeButtonImage;
    public Image controlsButtonImage;
    public Image menuButtonImage;

    [Header("Button Sprites (Normal)")]
    public Sprite resumeNormal;
    public Sprite controlsNormal;
    public Sprite menuNormal;

    [Header("Button Sprites (Pressed)")]
    public Sprite resumePressed;
    public Sprite controlsPressed;
    public Sprite menuPressed;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenuScene";

    public bool IsPaused { get; private set; } = false;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (controlsOverlay != null) controlsOverlay.SetActive(false);
    }

    private void Update()
    {
        // Press Escape to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    // -------------------------------------------------------
    // Pause / Resume
    // -------------------------------------------------------

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f; // Freeze the game
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (controlsOverlay != null) controlsOverlay.SetActive(false);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f; // Unfreeze the game
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (controlsOverlay != null) controlsOverlay.SetActive(false);
    }

    // -------------------------------------------------------
    // Button callbacks
    // -------------------------------------------------------

    public void OnResumePressed()
    {
        Resume();
    }

    public void OnControlsPressed()
    {
        if (controlsOverlay != null)
            controlsOverlay.SetActive(true);
    }

    public void OnMenuPressed()
    {
        // Restore time scale before loading — otherwise the
        // main menu loads with the game still frozen
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void CloseControls()
    {
        if (controlsOverlay != null)
            controlsOverlay.SetActive(false);
    }
}