// CharacterSelectManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character Data (assign all 4 in order)")]
    public CharacterDataSO[] characters;

    [Header("Preview UI")]
    public Image previewSprite;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterDescriptionText;
    public TextMeshProUGUI statsText;

    [Header("Button Images")]
    public Image startButtonImage;
    public Image wizardButtonImage;
    public Image clericButtonImage;
    public Image rogueButtonImage;
    public Image barbarianButtonImage;
    public Image backButtonImage;

    [Header("Button Sprites (Normal)")]
    public Sprite startNormal;
    public Sprite wizardNormal;
    public Sprite clericNormal;
    public Sprite rogueNormal;
    public Sprite barbarianNormal;
    public Sprite backNormal;

    [Header("Button Sprites (Pressed)")]
    public Sprite startPressed;
    public Sprite wizardPressed;
    public Sprite clericPressed;
    public Sprite roguePressed;
    public Sprite barbarianPressed;
    public Sprite backPressed;

    [Header("Scene Names")]
    public string gameSceneName = "GameScene";
    public string mainMenuSceneName = "MainMenu";

    private int selectedIndex = 0;

    private void Start()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("CharacterSelectManager: No characters assigned!");
            return;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null)
                Debug.LogError($"CharacterSelectManager: Element {i} in Characters array is null!");
            else
                Debug.Log($"CharacterSelectManager: Slot {i} = {characters[i].characterName}");
        }

        PreviewCharacter(0);
    }

    // -------------------------------------------------------
    // Button callbacks
    // -------------------------------------------------------

    public void OnStartPressed()
    {
        if (characters == null || selectedIndex >= characters.Length)
        {
            Debug.LogError("CharacterSelectManager: characters array is null or selectedIndex out of range!");
            return;
        }

        if (characters[selectedIndex] == null)
        {
            Debug.LogError($"CharacterSelectManager: characters[{selectedIndex}] is null!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("CharacterSelectManager: GameManager.Instance is null!");
            return;
        }

        GameManager.Instance.SetSelectedCharacter(characters[selectedIndex]);
        Debug.Log($"Confirmed: {characters[selectedIndex].characterName} — loading {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnWizardPressed()    => SelectCharacter(0);
    public void OnClericPressed()    => SelectCharacter(1);
    public void OnRoguePressed()     => SelectCharacter(2);
    public void OnBarbarianPressed() => SelectCharacter(3);

    public void OnBackPressed()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // -------------------------------------------------------
    // Character selection
    // -------------------------------------------------------

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
        {
            Debug.LogError($"CharacterSelectManager: Index {index} is out of range!");
            return;
        }

        if (characters[index] == null)
        {
            Debug.LogError($"CharacterSelectManager: Character at index {index} is null!");
            return;
        }

        selectedIndex = index;
        PreviewCharacter(index);
        Debug.Log($"Selected character: {characters[index].characterName}");
    }

    private void PreviewCharacter(int index)
    {
        CharacterDataSO data = characters[index];
        if (data == null) return;

        if (previewSprite != null && data.characterSprite != null)
            previewSprite.sprite = data.characterSprite;

        if (characterNameText != null)
            characterNameText.text = data.characterName;

        if (characterDescriptionText != null)
            characterDescriptionText.text = data.characterDescription;

        if (statsText != null)
        {
            statsText.text =
                $"Speed:       {data.moveSpeed}\n" +
                $"Mobility:    {data.mobilityMultiplier}x\n" +
                $"Health:      {data.maxHealth}\n" +
                $"Melee Dmg:   {data.baseMeleeDamage}\n" +
                $"Range Dmg:   {data.baseRangedDamage}";
        }
    }
}