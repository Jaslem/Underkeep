// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------
    // Singleton
    // -------------------------------------------------------
    public static GameManager Instance { get; private set; }

    [Header("Selected Character (set by CharacterSelectManager)")]
    public CharacterDataSO selectedCharacter;

    private void Awake()
    {
        // Classic persistent singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSelectedCharacter(CharacterDataSO data)
    {
        selectedCharacter = data;
    }
}
