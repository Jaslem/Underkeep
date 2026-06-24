// MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public Image logoImage;
    public Image gameNameImage;

    [Header("Button Images")]
    public Image startButtonImage;
    public Image controlsButtonImage;
    public Image creditsButtonImage;
    public Image quitButtonImage;

    [Header("Button Sprites (Normal)")]
    public Sprite startNormal;
    public Sprite controlsNormal;
    public Sprite creditsNormal;
    public Sprite quitNormal;

    [Header("Button Sprites (Pressed)")]
    public Sprite startPressed;
    public Sprite controlsPressed;
    public Sprite creditsPressed;
    public Sprite quitPressed;

    [Header("Overlay Panels")]
    public GameObject controlsOverlay;
    public GameObject creditsOverlay;

    [Header("Scene Names")]
    public string characterSelectScene = "CharacterSelect";

    private void Start()
    {
        // Make sure overlays are hidden on start
        if (controlsOverlay != null) controlsOverlay.SetActive(false);
        if (creditsOverlay  != null) creditsOverlay.SetActive(false);

        // Set all buttons to normal state
        ResetAllButtons();
    }

    // -------------------------------------------------------
    // Button callbacks — wire these to your Button components
    // -------------------------------------------------------

    public void OnStartPressed()
    {
        SceneManager.LoadScene(characterSelectScene);
    }

    public void OnControlsPressed()
    {
        // Close credits if open, open controls
        if (creditsOverlay  != null) creditsOverlay.SetActive(false);
        if (controlsOverlay != null) controlsOverlay.SetActive(true);
    }

    public void OnCreditsPressed()
    {
        // Close controls if open, open credits
        if (controlsOverlay != null) controlsOverlay.SetActive(false);
        if (creditsOverlay  != null) creditsOverlay.SetActive(true);
    }

    public void OnQuitPressed()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void CloseOverlays()
    {
        if (controlsOverlay != null) controlsOverlay.SetActive(false);
        if (creditsOverlay  != null) creditsOverlay.SetActive(false);
    }

    // -------------------------------------------------------
    // Button press / release visual feedback
    // Called by EventTrigger components on each button
    // -------------------------------------------------------

    public void OnStartDown()    => SetButtonSprite(startButtonImage,    startPressed);
    public void OnStartUp()      => SetButtonSprite(startButtonImage,    startNormal);

    public void OnControlsDown() => SetButtonSprite(controlsButtonImage, controlsPressed);
    public void OnControlsUp()   => SetButtonSprite(controlsButtonImage, controlsNormal);

    public void OnCreditsDown()  => SetButtonSprite(creditsButtonImage,  creditsPressed);
    public void OnCreditsUp()    => SetButtonSprite(creditsButtonImage,  creditsNormal);

    public void OnQuitDown()     => SetButtonSprite(quitButtonImage,     quitPressed);
    public void OnQuitUp()       => SetButtonSprite(quitButtonImage,     quitNormal);

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void SetButtonSprite(Image image, Sprite sprite)
    {
        if (image != null && sprite != null)
            image.sprite = sprite;
    }

    private void ResetAllButtons()
    {
        SetButtonSprite(startButtonImage,    startNormal);
        SetButtonSprite(controlsButtonImage, controlsNormal);
        SetButtonSprite(creditsButtonImage,  creditsNormal);
        SetButtonSprite(quitButtonImage,     quitNormal);
    }
}