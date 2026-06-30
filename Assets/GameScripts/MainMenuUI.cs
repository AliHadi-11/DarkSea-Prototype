using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    VisualElement _root;
    Label         _welcomeLabel;

    // Scores panel
    VisualElement _scoresPanel;
    VisualElement _scoresList;

    // Settings panel
    VisualElement _settingsPanel;
    TextField     _settingsName;
    TextField     _settingsAge;
    Button        _btnPovFP, _btnPovTP;
    Button        _btnDiffEasy, _btnDiffNormal, _btnDiffHard;
    Button        _btnFullscreen, _btnWindowed;
    Label         _settingsFeedback;
    int           _selectedPOV;
    int           _selectedDiff;

    // Keyboard navigation
    struct NavEntry { public Button btn; public System.Action action; }
    readonly System.Collections.Generic.List<NavEntry> _nav = new System.Collections.Generic.List<NavEntry>();
    int _kbIdx = -1;

    void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        if (_root == null) { Debug.LogError("[MainMenuUI] rootVisualElement null."); return; }

        _welcomeLabel = _root.Q<Label>("welcome-label");

        // Scores
        _scoresPanel = _root.Q<VisualElement>("scores-panel");
        _scoresList  = _root.Q<VisualElement>("scores-list");

        // Settings
        _settingsPanel    = _root.Q<VisualElement>("settings-panel");
        _settingsName     = _root.Q<TextField>("settings-name");
        _settingsAge      = _root.Q<TextField>("settings-age");
        _btnPovFP = _root.Q<Button>("btn-pov-fp");
        _btnPovTP = _root.Q<Button>("btn-pov-tp");
        _btnDiffEasy   = _root.Q<Button>("btn-diff-easy");
        _btnDiffNormal = _root.Q<Button>("btn-diff-normal");
        _btnDiffHard   = _root.Q<Button>("btn-diff-hard");
        _btnFullscreen = _root.Q<Button>("btn-fullscreen");
        _btnWindowed   = _root.Q<Button>("btn-windowed");
        _settingsFeedback = _root.Q<Label>("settings-feedback");

        // Placeholders
        try
        {
            if (_settingsName != null) _settingsName.textEdition.placeholder = "e.g. DarkHero";
            if (_settingsAge  != null) _settingsAge.textEdition.placeholder  = "e.g. 18";
        }
        catch { }

        // Hide legacy name-prompt overlay
        var namePrompt = _root.Q<VisualElement>("name-prompt");
        if (namePrompt != null) namePrompt.style.display = DisplayStyle.None;

        // Welcome message
        string saved = PlayerPrefs.GetString("CurrentPlayer", "").Trim();
        if (!string.IsNullOrEmpty(saved)) SetWelcome(saved);

        // Settings events
        var btnSave  = _root.Q<Button>("btn-settings-save");
        var btnClose = _root.Q<Button>("btn-settings-close");
        if (btnSave  != null) btnSave.clicked  += OnSettingsSave;
        if (btnClose != null) btnClose.clicked += CloseSettings;
        if (_btnPovFP != null) _btnPovFP.clicked += () => { _selectedPOV = 0; UpdatePOVButtons(); };
        if (_btnPovTP != null) _btnPovTP.clicked += () => { _selectedPOV = 1; UpdatePOVButtons(); };

        if (_btnDiffEasy   != null) _btnDiffEasy.clicked   += () => { _selectedDiff = 0; UpdateDiffButtons(); };
        if (_btnDiffNormal != null) _btnDiffNormal.clicked += () => { _selectedDiff = 1; UpdateDiffButtons(); };
        if (_btnDiffHard   != null) _btnDiffHard.clicked   += () => { _selectedDiff = 2; UpdateDiffButtons(); };

        if (_btnFullscreen != null) _btnFullscreen.clicked += () => { Screen.fullScreen = true;  UpdateDisplayButtons(); };
        if (_btnWindowed   != null) _btnWindowed.clicked   += () => { Screen.fullScreen = false; UpdateDisplayButtons(); };

        // Scores close
        var btnCloseScores = _root.Q<Button>("btn-close-scores");
        if (btnCloseScores != null) btnCloseScores.clicked += HideScoresPanel;

        // Nav buttons
        Bind("btn-play",         OnPlay);
        Bind("btn-settings",     OpenSettings);
        Bind("btn-achievements", OnScores);
        Bind("btn-exit",         OnExit);

        // Build keyboard nav list (same order as visual buttons)
        _nav.Clear();
        void AddNav(string name, System.Action cb) {
            var b = _root.Q<Button>(name);
            if (b != null) _nav.Add(new NavEntry { btn = b, action = cb });
        }
        AddNav("btn-play",         OnPlay);
        AddNav("btn-settings",     OpenSettings);
        AddNav("btn-achievements", OnScores);
        AddNav("btn-exit",         OnExit);

    }

    // ── Keyboard navigation ────────────────────────────────────────────────────

    void Update()
    {
        if (_root == null || _nav.Count == 0) return;
        // Overlays open — skip menu navigation
        if (_settingsPanel?.style.display == DisplayStyle.Flex) return;
        if (_scoresPanel?.style.display   == DisplayStyle.Flex) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            SetMenuFocus((_kbIdx <= 0 ? _nav.Count : _kbIdx) - 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            SetMenuFocus((_kbIdx + 1) % _nav.Count);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_kbIdx >= 0 && _kbIdx < _nav.Count) _nav[_kbIdx].action?.Invoke();
        }
    }

    void SetMenuFocus(int idx)
    {
        if (_kbIdx >= 0 && _kbIdx < _nav.Count)
            _nav[_kbIdx].btn.RemoveFromClassList("menu-button-kb-focus");
        _kbIdx = idx;
        if (_kbIdx >= 0 && _kbIdx < _nav.Count)
            _nav[_kbIdx].btn.AddToClassList("menu-button-kb-focus");
    }

    void SetWelcome(string name)
    {
        if (_welcomeLabel != null)
            _welcomeLabel.text = "WELCOME BACK, " + name.ToUpper();
    }

    // ── Settings panel ─────────────────────────────────────────────

    void OpenSettings()
    {
        if (_settingsPanel == null) return;

        // Load saved values
        if (_settingsName != null)
            _settingsName.value = PlayerPrefs.GetString("CurrentPlayer", "").Trim();

        int savedAge = PlayerData.GetInt("PlayerAge", 0);
        if (_settingsAge != null)
            _settingsAge.value = savedAge > 0 ? savedAge.ToString() : "";

        _selectedPOV  = PlayerData.GetInt("PrefPOV", 0);
        _selectedDiff = (int)DifficultyManager.Current;
        UpdatePOVButtons();
        UpdateDiffButtons();
        UpdateDisplayButtons();

        if (_settingsFeedback != null) _settingsFeedback.text = "";
        _settingsPanel.style.display = DisplayStyle.Flex;
    }

    void CloseSettings()
    {
        if (_settingsPanel != null) _settingsPanel.style.display = DisplayStyle.None;
    }

    void OnSettingsSave()
    {
        string name = _settingsName?.value.Trim() ?? "";

        if (name.Length < 2)
        {
            ShowFeedback("Name must be at least 2 characters.", error: true);
            return;
        }
        if (name.Length > 20)
        {
            ShowFeedback("Name must be 20 characters or fewer.", error: true);
            return;
        }

        // Save name
        PlayerPrefs.SetString("CurrentPlayer", name);

        // Save age (optional — silently ignored if invalid)
        string ageStr = _settingsAge?.value.Trim() ?? "";
        if (int.TryParse(ageStr, out int age) && age >= 5 && age < 120)
            PlayerData.SetInt("PlayerAge", age);

        // Save POV preference
        PlayerData.SetInt("PrefPOV", _selectedPOV);
        // Save difficulty
        DifficultyManager.Set((Difficulty)_selectedDiff);
        PlayerPrefs.Save();

        SetWelcome(name);
        CloseSettings();
    }

    void UpdatePOVButtons()
    {
        void Refresh(Button btn, int idx)
        {
            if (btn == null) return;
            btn.RemoveFromClassList("settings-pov-active");
            if (_selectedPOV == idx) btn.AddToClassList("settings-pov-active");
        }
        Refresh(_btnPovFP, 0);
        Refresh(_btnPovTP, 1);
    }

    void UpdateDiffButtons()
    {
        void Refresh(Button btn, int idx)
        {
            if (btn == null) return;
            btn.RemoveFromClassList("settings-pov-active");
            if (_selectedDiff == idx) btn.AddToClassList("settings-pov-active");
        }
        Refresh(_btnDiffEasy,   0);
        Refresh(_btnDiffNormal, 1);
        Refresh(_btnDiffHard,   2);
    }

    void UpdateDisplayButtons()
    {
        bool fs = Screen.fullScreen;
        if (_btnFullscreen != null)
        {
            _btnFullscreen.RemoveFromClassList("settings-pov-active");
            if (fs)  _btnFullscreen.AddToClassList("settings-pov-active");
        }
        if (_btnWindowed != null)
        {
            _btnWindowed.RemoveFromClassList("settings-pov-active");
            if (!fs) _btnWindowed.AddToClassList("settings-pov-active");
        }
    }

    void ShowFeedback(string msg, bool error = false)
    {
        if (_settingsFeedback == null) return;
        _settingsFeedback.text = msg;
        if (error)
        {
            _settingsFeedback.RemoveFromClassList("settings-feedback");
            _settingsFeedback.AddToClassList("settings-feedback-error");
        }
        else
        {
            _settingsFeedback.RemoveFromClassList("settings-feedback-error");
            _settingsFeedback.AddToClassList("settings-feedback");
        }
    }

    // ── Scores panel ────────────────────────────────────────────────

    void OnScores()
    {
        if (_scoresPanel == null || _scoresList == null) return;
        _scoresList.Clear();

        string[] entries = ScoreHistory.GetAll();
        if (entries.Length == 0)
        {
            var empty = new Label { text = "No scores yet. Play a level first!" };
            empty.AddToClassList("score-row-empty");
            _scoresList.Add(empty);
        }
        else
        {
            string[] levelNames = { "", "Level 1", "Level 2", "Level 3" };
            for (int i = 0; i < entries.Length; i++)
            {
                string[] p = entries[i].Split('|');
                if (p.Length < 3) continue;
                int  lvl   = int.TryParse(p[0], out int l) ? l : 0;
                int  score = int.TryParse(p[1], out int s) ? s : 0;
                bool won   = p[2] == "W";
                string lName = lvl >= 1 && lvl <= 3 ? levelNames[lvl] : "Level " + lvl;
                var row = new Label
                {
                    text = "#" + (i + 1) + "  " + lName + "   " + score + " pts   [" + (won ? "WIN" : "LOSE") + "]"
                };
                row.AddToClassList("score-row");
                row.AddToClassList(won ? "score-win" : "score-lose");
                _scoresList.Add(row);
            }
        }
        // ── Achievements ─────────────────────────────────────────────
        var sep = new Label("── ACHIEVEMENTS ──");
        sep.AddToClassList("score-row-empty");
        sep.style.marginTop = 8;
        _scoresList.Add(sep);

        foreach (var ach in AchievementSystem.All)
        {
            bool unlocked = AchievementSystem.IsUnlocked(ach.id);
            var row = new Label((unlocked ? "★  " : "○  ") + ach.title + "  —  " + ach.desc);
            row.AddToClassList("score-row");
            row.AddToClassList(unlocked ? "score-win" : "score-lose");
            _scoresList.Add(row);
        }

        _scoresPanel.style.display = DisplayStyle.Flex;
    }

    void HideScoresPanel()
    {
        if (_scoresPanel != null) _scoresPanel.style.display = DisplayStyle.None;
    }

    // ── Nav buttons ─────────────────────────────────────────────────

    void OnPlay()
    {
        string savedName = PlayerPrefs.GetString("CurrentPlayer", "").Trim();
        if (string.IsNullOrEmpty(savedName))
        {
            if (_settingsFeedback != null)
                _settingsFeedback.text = "Please enter your player name first.";
            OpenSettings();
            return;
        }
        SceneFader.FadeAndLoad("LevelSelect");
    }

    void OnExit() { Application.Quit(); }

    void Bind(string buttonName, System.Action callback)
    {
        var btn = _root.Q<Button>(buttonName);
        if (btn != null) btn.clicked += callback;
        else Debug.LogWarning("[MainMenuUI] Button not found: " + buttonName);
    }
}
