using UnityEngine;
using TMPro; // Text ke liye
using UnityEngine.SceneManagement; // Level Reload karne ke liye zaroori

public class OxygenSystem : MonoBehaviour
{
    [Header("Settings")]
    public float currentOxygen = 100f; // Oxygen ki miqdaar
    public float oxygenDecreaseSpeed = 2f; // Oxygen kitni tezi se kam ho

    [Header("UI Links")]
    public TMP_Text oxygenText;
    public GameObject warningText;
    public GameOverUI gameOverUI;

    public static bool IsGameOver = false;
    private bool isDead = false;
    private bool warnedAt30 = false;
    private bool warnedAt20 = false;

    void Start()
    {
        oxygenDecreaseSpeed *= DifficultyManager.OxygenDrainMultiplier;
    }

    void Update()
    {
        if (isDead) return;

        // --- OXYGEN KAM KARNA ---
        if (currentOxygen > 0)
        {
            currentOxygen -= oxygenDecreaseSpeed * Time.deltaTime;
        }

        // --- UI UPDATE & WARNING LOGIC ---
        if (oxygenText != null)
        {
            oxygenText.text = "Oxygen: " + currentOxygen.ToString("F0") + "%";

            if (currentOxygen <= 20 && currentOxygen > 0)
            {
                if (warningText != null) warningText.SetActive(true);
                oxygenText.color = Color.red;
            }
            else
            {
                if (warningText != null) warningText.SetActive(false);
                oxygenText.color = Color.white;
            }
        }

        // --- OXYGEN WARNING NOTIFICATIONS ---
        if (!isDead)
        {
            if (currentOxygen <= 30f && !warnedAt30)
            {
                warnedAt30 = true;
                NotificationUI.Show("LOW OXYGEN!", NotificationUI.NotifType.Warning);
            }
            if (currentOxygen <= 20f && !warnedAt20)
            {
                warnedAt20 = true;
                NotificationUI.Show("CRITICAL OXYGEN!", NotificationUI.NotifType.Danger);
                CameraShake.Shake(0.35f, 0.10f);
            }
        }

        // --- GAME OVER (oxygen khatam) ---
        if (currentOxygen <= 0)
        {
            ShowGameOver("OUT OF OXYGEN!");
        }
    }

    // Enemy ya koi bhi script isko call kare apne reason ke saath.
    public void ShowGameOver(string reason)
    {
        if (isDead) return;
        isDead = true;
        IsGameOver = true;
        currentOxygen = 0;
        AudioManager.StopAll();
        CameraShake.Shake(0.50f, 0.18f);

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int lvl = scene.Contains("Level_2") ? 2 : scene.Contains("Level_3") ? 3 : 1;
        ScoreHistory.Push(lvl, 0, false);

        if (warningText != null) warningText.SetActive(false);

        Debug.Log("Game Over! " + reason);

        if (gameOverUI != null)
            gameOverUI.Show(reason);
        else
        {
            // Fallback agar GameOverUI assign nahi
            Time.timeScale = 0;
            AudioListener.pause = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // --- RESTART BUTTON FUNCTION ---
    public void RestartGame()
    {
        IsGameOver = false;
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Cylinder uthane ke liye
    public void RefillOxygen(float amount)
    {
        currentOxygen += amount;
        if (currentOxygen > 100) currentOxygen = 100;
    }
}