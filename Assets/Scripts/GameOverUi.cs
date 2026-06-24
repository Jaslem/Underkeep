// GameOverUI.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles both the Victory and Defeat screens.
/// Attach to a GameObject on the Canvas.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Victory Screen")]
    public GameObject victoryPanel;
    public Image      victoryImage;
    public Sprite     victorySprite;
    public Image      victoryMainMenuButtonImage;
    public Sprite     victoryButtonNormal;
    public Sprite     victoryButtonPressed;

    [Header("Defeat Screen")]
    public GameObject defeatPanel;
    public Image      defeatImage;
    public Sprite     defeatSprite;
    public Image      defeatMainMenuButtonImage;
    public Sprite     defeatButtonNormal;
    public Sprite     defeatButtonPressed;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenuScene";

    private void Start()
    {
        // Make sure both panels are hidden at start
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel  != null) defeatPanel.SetActive(false);

        // Subscribe to player death
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
                playerStats.OnPlayerDied.AddListener(ShowDefeat);
        }
    }

    // -------------------------------------------------------
    // Show Screens
    // -------------------------------------------------------

    public void ShowVictory()
    {
        Time.timeScale = 0f;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryImage != null && victorySprite != null)
                victoryImage.sprite = victorySprite;
        }

        Debug.Log("[GameOverUI] Victory screen shown");
    }

    public void ShowDefeat()
    {
        Time.timeScale = 0f;

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);

            if (defeatImage != null && defeatSprite != null)
                defeatImage.sprite = defeatSprite;
        }

        Debug.Log("[GameOverUI] Defeat screen shown");
    }

    // -------------------------------------------------------
    // Button Callbacks
    // -------------------------------------------------------

    public void OnVictoryMainMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnDefeatMainMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Button press visual feedback
    public void OnVictoryButtonDown()   => SetButtonSprite(victoryMainMenuButtonImage, victoryButtonPressed);
    public void OnVictoryButtonUp()     => SetButtonSprite(victoryMainMenuButtonImage, victoryButtonNormal);
    public void OnDefeatButtonDown()    => SetButtonSprite(defeatMainMenuButtonImage,  defeatButtonPressed);
    public void OnDefeatButtonUp()      => SetButtonSprite(defeatMainMenuButtonImage,  defeatButtonNormal);

    private void SetButtonSprite(Image image, Sprite sprite)
    {
        if (image != null && sprite != null)
            image.sprite = sprite;
    }

    private void OnDestroy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
                playerStats.OnPlayerDied.RemoveListener(ShowDefeat);
        }
    }
}