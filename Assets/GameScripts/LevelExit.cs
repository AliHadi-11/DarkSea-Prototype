using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
// =========================================================
//  Dark Sea - Level Exit / Finish + SCORE + UNLOCK
//  Exit Gate par lagao.
//
//  SETUP:
//   - Exit Gate par Box Collider + "Is Trigger" ON
//   - Level Number: DarkSea=1, Level2=2, Level3=3
//   - Next Scene Name: Level 1->"Level2_Transition", Level 2->"Level3_Transition"
//     Level 3 (aakhri): Is Final Level = ON, Next Scene = blank
//   - Win Panel UI: WinPanelUI component wala GameObject drag karo
//   - Player = Player drag karo
//
//  Score = (bachi oxygen x10) + time bonus + (tanks x50)
// =========================================================
public class LevelExit : MonoBehaviour
{
    [Header("This Level")]
    public int levelNumber = 1;          // DarkSea=1, Level_2=2, Level_3=3

    [Header("Progression")]
    public string nextSceneName = "";    // agla level (Build Settings mein ho)
    public bool isFinalLevel = false;    // aakhri level par ON

    [Header("Oxygen Tanks (Level 2)")]
    public bool requireTanks = false;
    public int requiredTanks = 3;

    [Header("UI")]
    public WinPanelUI winPanelUI;        // WinPanelUI component wala GameObject drag karo
    public OxygenSystem oxygenScript;    // Player ki oxygen (score ke liye)

    [Header("Win par rokne ke liye")]
    public GameObject player;
    public GameObject[] enemies;

    private bool finished = false;
    private float startTime;

    void Awake()
    {
        TankCollector.Collected = 0;
    }

    void Start()
    {
        startTime = Time.time;

        // Auto-find any Inspector refs that were left unassigned
        if (oxygenScript == null)
            oxygenScript = Object.FindFirstObjectByType<OxygenSystem>();
        if (winPanelUI == null)
            winPanelUI = Object.FindFirstObjectByType<WinPanelUI>();
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p;
        }

        Debug.Log("LevelExit ready on: " + gameObject.name + " (Level " + levelNumber + ")");
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished) return;

        // Player ya uske child (jaise MapIcon) - dono ko Player maano
        bool isPlayer = other.CompareTag("Player") ||
                        (other.GetComponentInParent<PlayerMovementFinal>() != null);
        if (!isPlayer) return;

        // Tank gate (Level 2)
        if (requireTanks && TankCollector.Collected < requiredTanks)
        {
            GameMessage.Show("Pehle " + requiredTanks + " oxygen tanks collect karo! (" +
                             TankCollector.Collected + "/" + requiredTanks + ")");
            return;
        }

        finished = true;

        // Agla level unlock karo
        UnlockNext();

        if (isFinalLevel || string.IsNullOrEmpty(nextSceneName))
        {
            ShowComplete(true); // aakhri level / kahin nahi jaana
        }
        else
        {
            ShowComplete(false); // normal level complete (Next button milega)
        }
    }

    void UnlockNext()
    {
        int unlocked = PlayerData.GetInt("UnlockedLevel", 1);
        int next = levelNumber + 1;
        if (next > unlocked)
        {
            PlayerData.SetInt("UnlockedLevel", next);
            PlayerPrefs.Save();
        }
    }

    void ShowComplete(bool isFinal)
    {
        AudioManager.StopAll();
        StopEverything();

        string playerName = PlayerPrefs.GetString("CurrentPlayer",
                            PlayerPrefs.GetString("SavedName", "Player"));

        int oxygen = 0;
        if (oxygenScript != null) oxygen = Mathf.Max(0, (int)oxygenScript.currentOxygen);

        int elapsed   = (int)(Time.time - startTime);
        int timeBonus = Mathf.Max(0, 200 - elapsed) * 2;
        int tanks     = TankCollector.Collected;
        int total     = (oxygen * 10) + timeBonus + (tanks * 50);

        int best = PlayerData.GetInt("BestScore_L" + levelNumber, 0);
        if (total > best) PlayerData.SetInt("BestScore_L" + levelNumber, total);
        ScoreHistory.Push(levelNumber, total, true);
        PlayerPrefs.Save();

        // Achievement checks
        if (oxygen >= 60) AchievementSystem.Unlock("IronLungs");
        if (elapsed < 90) AchievementSystem.Unlock("SpeedRun");
        if (levelNumber == 2 && tanks >= requiredTanks) AchievementSystem.Unlock("TankMaster");
        if (isFinalLevel) AchievementSystem.Unlock("DeepDiver");

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winPanelUI != null)
            winPanelUI.Show(playerName, total, oxygen, isFinal, nextSceneName);
        else
            Debug.LogWarning("[LevelExit] WinPanelUI not assigned in Inspector.");
    }

    void StopEverything()
    {
        if (player != null)
        {
            PlayerMovementFinal pm = player.GetComponent<PlayerMovementFinal>();
            if (pm != null) pm.enabled = false;
        }

        if (enemies != null)
            foreach (GameObject e in enemies) StopOneEnemy(e);

        if (enemies == null || enemies.Length == 0)
        {
            GameObject[] found = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject e in found) StopOneEnemy(e);
        }
    }

    void StopOneEnemy(GameObject e)
    {
        if (e == null) return;
        EnemyAI_Final ai = e.GetComponent<EnemyAI_Final>();
        if (ai != null) ai.enabled = false;
        NavMeshAgent agent = e.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh) { agent.isStopped = true; agent.velocity = Vector3.zero; }
        AudioSource au = e.GetComponent<AudioSource>();
        if (au != null) au.Stop();
    }

    // ===== PANEL BUTTONS =====

    // "Next Level" button
    public void NextLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene("MainMenu"); // agla nahi to menu
    }

    // "Retry" button
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // "Main Menu" button
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

// Score history — stored per-user via PlayerData.Key.
// Format: comma-separated "level|score|W" or "level|score|L", newest first, max 5.
public static class ScoreHistory
{
    const string BASE_KEY = "RecentScores";
    const int    MAX      = 5;

    public static void Push(int level, int score, bool won)
    {
        string entry = level + "|" + score + "|" + (won ? "W" : "L");
        string key   = PlayerData.Key(BASE_KEY);
        string raw   = PlayerPrefs.GetString(key, "");

        var list = new System.Collections.Generic.List<string>();
        list.Add(entry);

        if (!string.IsNullOrEmpty(raw))
            foreach (string s in raw.Split(','))
                if (!string.IsNullOrEmpty(s) && list.Count < MAX)
                    list.Add(s);

        PlayerPrefs.SetString(key, string.Join(",", list));
        PlayerPrefs.Save();
    }

    public static string[] GetAll()
    {
        string raw = PlayerPrefs.GetString(PlayerData.Key(BASE_KEY), "");
        if (string.IsNullOrEmpty(raw)) return new string[0];
        return raw.Split(',');
    }
}

// Prefixes every game-progress key with the logged-in username
// so different accounts never share unlock state, scores, or history.
public static class PlayerData
{
    public static string Key(string baseKey)
    {
        string user = PlayerPrefs.GetString("CurrentPlayer", "").Trim();
        return string.IsNullOrEmpty(user) ? baseKey : "U_" + user + "_" + baseKey;
    }

    public static int    GetInt(string key, int def = 0)
        => PlayerPrefs.GetInt(Key(key), def);

    public static void   SetInt(string key, int val)
        => PlayerPrefs.SetInt(Key(key), val);
}