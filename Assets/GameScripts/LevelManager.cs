using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public GameObject winPanel; // "Mission Passed" wala panel

    void Awake()
    {
        instance = this;
    }

    // Ye function EnemyAI call karega jab wo marega
    public void EnemyKilled()
    {
        // Koi ginti nahi, seedha Jeet jao
        WinGame();
    }

    void WinGame()
    {
        Debug.Log("Mission Passed!");

        // Panel show karo
        if (winPanel != null) winPanel.SetActive(true);

        // Game rok do aur Mouse wapis lao
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void Start()
    {
        // Shuru mein Panel ko chupao
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }
}