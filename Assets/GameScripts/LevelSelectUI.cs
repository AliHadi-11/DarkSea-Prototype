using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

// =========================================================
//  Dark Sea - Level Select (UI Toolkit)
//
//  PlayerPrefs["UnlockedLevel"] (default 1):
//    level < unlocked  -> COMPLETED (green, clickable)
//    level == unlocked -> AVAILABLE (blue,  clickable)
//    level > unlocked  -> LOCKED    (grey,  disabled)
//
//  BACK -> MainMenu scene
// =========================================================
[RequireComponent(typeof(UIDocument))]
public class LevelSelectUI : MonoBehaviour
{
    static readonly string[] LevelScenes = { "Level1_Transition", "Level2_Transition", "Level3_Transition" };

    struct NavEntry { public Button btn; public System.Action action; public string focusClass; }
    readonly System.Collections.Generic.List<NavEntry> _nav = new System.Collections.Generic.List<NavEntry>();
    int _kbIdx = -1;

    // OnEnable fires after UIDocument.OnEnable, so rootVisualElement
    // is already populated when we query it here.
    void OnEnable()
    {
        Debug.Log("[LevelSelectUI] OnEnable called.");

        var doc  = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("[LevelSelectUI] rootVisualElement is null — UIDocument may not have a VisualTreeAsset assigned.");
            return;
        }

        int unlocked = PlayerData.GetInt("UnlockedLevel", 1);
        Debug.Log("[LevelSelectUI] UnlockedLevel = " + unlocked);

        for (int i = 1; i <= 3; i++)
            SetupLevelButton(root, i, unlocked);

        var back = root.Q<Button>("btn-back");
        if (back != null)
        {
            back.clicked += OnBack;
            Debug.Log("[LevelSelectUI] Registered: btn-back");
        }
        else
        {
            Debug.LogError("[LevelSelectUI] Button NOT found in UXML: btn-back");
        }

        Debug.Log("[LevelSelectUI] All button callbacks registered.");

        // ── Keyboard navigation ─────────────────────────
        _nav.Clear();
        for (int i = 1; i <= 3; i++)
        {
            var b = root.Q<Button>("btn-level-" + i);
            if (b != null && b.enabledSelf)
            {
                int cap = i;
                _nav.Add(new NavEntry {
                    btn        = b,
                    action     = () => SceneFader.FadeAndLoad(LevelScenes[cap - 1]),
                    focusClass = "level-button-kb-focus"
                });
            }
        }
        if (back != null)
            _nav.Add(new NavEntry { btn = back, action = OnBack, focusClass = "back-button-kb-focus" });

    }

    // ── Keyboard navigation ────────────────────────────────

    void Update()
    {
        if (_nav.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            SetLevelFocus((_kbIdx <= 0 ? _nav.Count : _kbIdx) - 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            SetLevelFocus((_kbIdx + 1) % _nav.Count);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_kbIdx >= 0 && _kbIdx < _nav.Count) _nav[_kbIdx].action?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            OnBack();
    }

    void SetLevelFocus(int idx)
    {
        if (_kbIdx >= 0 && _kbIdx < _nav.Count)
            _nav[_kbIdx].btn.RemoveFromClassList(_nav[_kbIdx].focusClass);
        _kbIdx = idx;
        if (_kbIdx >= 0 && _kbIdx < _nav.Count)
            _nav[_kbIdx].btn.AddToClassList(_nav[_kbIdx].focusClass);
    }

    // -------------------------------------------------------

    void SetupLevelButton(VisualElement root, int level, int unlocked)
    {
        var btn         = root.Q<Button>("btn-level-" + level);
        var statusLabel = root.Q<Label>("status-level-" + level);

        if (btn == null || statusLabel == null)
        {
            Debug.LogError("[LevelSelectUI] UI elements NOT found for level " + level +
                           " (btn=" + (btn == null ? "NULL" : "OK") +
                           ", label=" + (statusLabel == null ? "NULL" : "OK") + ")");
            return;
        }

        // Clear any stale USS classes set in UXML
        statusLabel.RemoveFromClassList("status-completed");
        statusLabel.RemoveFromClassList("status-available");
        statusLabel.RemoveFromClassList("status-locked");

        string sceneName    = LevelScenes[level - 1];
        int    captureLevel = level;   // capture for lambda

        if (level < unlocked)
        {
            int best = PlayerData.GetInt("BestScore_L" + level, 0);
            statusLabel.text = best > 0 ? "BEST: " + best : "COMPLETED";
            statusLabel.AddToClassList("status-completed");
            btn.SetEnabled(true);
            btn.clicked += () =>
            {
                Debug.Log("[LevelSelectUI] LEVEL " + captureLevel + " clicked (COMPLETED) -> loading " + sceneName);
                SceneFader.FadeAndLoad(sceneName);
            };
            Debug.Log("[LevelSelectUI] Registered: btn-level-" + level + " [COMPLETED] best=" + best);
        }
        else if (level == unlocked)
        {
            statusLabel.text = "PLAY";
            statusLabel.AddToClassList("status-available");
            btn.SetEnabled(true);
            btn.clicked += () =>
            {
                Debug.Log("[LevelSelectUI] LEVEL " + captureLevel + " clicked (AVAILABLE) -> loading " + sceneName);
                SceneFader.FadeAndLoad(sceneName);
            };
            Debug.Log("[LevelSelectUI] Registered: btn-level-" + level + " [AVAILABLE]");
        }
        else
        {
            statusLabel.text = "LOCKED";
            statusLabel.AddToClassList("status-locked");
            btn.SetEnabled(false);
            Debug.Log("[LevelSelectUI] btn-level-" + level + " disabled [LOCKED]");
        }
    }

    // -------------------------------------------------------

    void OnBack()
    {
        Debug.Log("[LevelSelectUI] BACK clicked -> loading MainMenu.");
        SceneFader.FadeAndLoad("MainMenu");
    }
}
