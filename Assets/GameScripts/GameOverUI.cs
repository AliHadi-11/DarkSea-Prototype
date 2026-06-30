using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class GameOverUI : MonoBehaviour
{
    VisualElement overlay;
    Label reasonLabel;
    bool initialized;

    void Start()
    {
        EnsureInitialized();
        if (overlay != null)
            overlay.style.display = DisplayStyle.None;
    }

    bool EnsureInitialized()
    {
        if (initialized) return overlay != null;

        var doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[GameOverUI] UIDocument component is missing.");
            return false;
        }

        var root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[GameOverUI] rootVisualElement is null — is GameOverPanel.uxml assigned?");
            return false;
        }

        overlay     = root.Q<VisualElement>("gameover-overlay");
        reasonLabel = root.Q<Label>("reason-label");

        var btnRestart = root.Q<Button>("btn-restart");
        var btnMenu    = root.Q<Button>("btn-menu");

        if (btnRestart != null) btnRestart.clicked += OnRestart;
        if (btnMenu    != null) btnMenu.clicked    += OnMenu;

        if (overlay == null)
            Debug.LogError("[GameOverUI] 'gameover-overlay' not found — check GameOverPanel.uxml.");

        initialized = true;
        return overlay != null;
    }

    public void Show(string reason)
    {
        if (!EnsureInitialized())
        {
            Debug.LogError("[GameOverUI] Show() failed — initialization error.");
            return;
        }

        overlay.style.display = DisplayStyle.Flex;

        if (reasonLabel != null)
            reasonLabel.text = reason;

        AudioManager.StopAll();
        Time.timeScale = 0f;
        AudioListener.pause = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    void OnRestart()
    {
        OxygenSystem.IsGameOver = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneFader.FadeAndLoad(SceneManager.GetActiveScene().name);
    }

    void OnMenu()
    {
        OxygenSystem.IsGameOver = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneFader.FadeAndLoad("MainMenu");
    }
}
