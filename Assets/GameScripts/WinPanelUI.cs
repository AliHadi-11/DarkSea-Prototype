using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class WinPanelUI : MonoBehaviour
{
    VisualElement overlay;
    Label titleLabel;
    Label playerNameValue;
    Label scoreValue;
    Label oxygenValue;
    Button btnNext;
    Button btnRetry;
    Button btnMenu;

    string nextScene;
    bool buttonsRegistered;

    // Start() fires after all OnEnable() calls (including UIDocument.OnEnable),
    // so rootVisualElement is guaranteed populated here.
    void Start()
    {
        if (EnsureInitialized())
            overlay.style.display = DisplayStyle.None;
    }

    bool EnsureInitialized()
    {
        if (overlay != null) return true;   // already cached

        var doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[WinPanelUI] UIDocument component is missing from this GameObject.");
            return false;
        }

        var root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[WinPanelUI] UIDocument.rootVisualElement is null — is WinPanel.uxml assigned?");
            return false;
        }

        overlay         = root.Q<VisualElement>("win-overlay");
        titleLabel      = root.Q<Label>("title-label");
        playerNameValue = root.Q<Label>("player-name-value");
        scoreValue      = root.Q<Label>("score-value");
        oxygenValue     = root.Q<Label>("oxygen-value");
        btnNext         = root.Q<Button>("btn-next");
        btnRetry        = root.Q<Button>("btn-retry");
        btnMenu         = root.Q<Button>("btn-menu");

        if (overlay == null)
        {
            Debug.LogError("[WinPanelUI] Element 'win-overlay' not found in UXML. " +
                           "Verify WinPanel.uxml is assigned and contains name=\"win-overlay\".");
            return false;
        }

        if (!buttonsRegistered)
        {
            if (btnRetry != null) btnRetry.clicked += OnRetry;
            if (btnMenu  != null) btnMenu.clicked  += OnMenu;
            buttonsRegistered = true;
        }

        return true;
    }

    public void Show(string playerName, int score, int oxygenPercent, bool isFinal, string next)
    {
        nextScene = next;

        // Guarantee cursor and time are correct regardless of call order
        AudioManager.StopAll();
        Time.timeScale       = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible   = true;

        if (!EnsureInitialized())
        {
            Debug.LogError("[WinPanelUI] Show() failed — could not initialize from UIDocument.");
            return;
        }

        overlay.style.display = DisplayStyle.Flex;

        // Fallback: read PlayerPrefs directly if caller passed an empty name
        if (string.IsNullOrEmpty(playerName))
            playerName = PlayerPrefs.GetString("CurrentPlayer",
                         PlayerPrefs.GetString("SavedName", "Player"));

        if (titleLabel      != null) titleLabel.text      = isFinal ? "GAME COMPLETE!" : "LEVEL COMPLETE!";
        if (playerNameValue != null) playerNameValue.text = playerName;
        if (scoreValue      != null) scoreValue.text      = score.ToString();
        if (oxygenValue     != null) oxygenValue.text     = oxygenPercent + "%";

        if (btnNext != null)
        {
            btnNext.style.display = isFinal ? DisplayStyle.None : DisplayStyle.Flex;
            if (!isFinal)
            {
                btnNext.clicked -= OnNext;
                btnNext.clicked += OnNext;
            }
        }

        // Credits section on final level
        if (isFinal)
        {
            var existing = overlay.Q<Label>("credits-block");
            if (existing == null)
            {
                var credits = new Label();
                credits.name = "credits-block";
                credits.text =
                    "\n━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                    "CREDITS\n\n" +
                    "Game Design & Development\nDarkSea Team\n\n" +
                    "Engine: Unity 6  |  Pipeline: URP\n\n" +
                    "Thank you for playing!\nDARKSEA: SURVIVAL  v1.0\n" +
                    "━━━━━━━━━━━━━━━━━━━━━━━━";
                credits.style.marginTop = 16;
                credits.style.color     = new UnityEngine.UIElements.StyleColor(
                    new UnityEngine.Color(0.45f, 0.60f, 0.52f));
                credits.style.fontSize  = 11;
                credits.style.whiteSpace = UnityEngine.UIElements.WhiteSpace.Normal;
                credits.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
                credits.pickingMode = UnityEngine.UIElements.PickingMode.Ignore;
                overlay.Add(credits);
            }
        }
    }

    void OnNext()
    {
        Time.timeScale = 1f;
        SceneFader.FadeAndLoad(!string.IsNullOrEmpty(nextScene) ? nextScene : "MainMenu");
    }

    void OnRetry()
    {
        Time.timeScale = 1f;
        SceneFader.FadeAndLoad(SceneManager.GetActiveScene().name);
    }

    void OnMenu()
    {
        Time.timeScale = 1f;
        SceneFader.FadeAndLoad("MainMenu");
    }
}
