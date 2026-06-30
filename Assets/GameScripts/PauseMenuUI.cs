using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuUI : MonoBehaviour
{
    VisualElement overlay;
    bool isPaused;
    bool initialized;

    void Start()
    {
        EnsureInitialized();
        if (overlay != null)
            overlay.style.display = DisplayStyle.None;
    }

    // Update runs every frame even when Time.timeScale == 0.
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.P)) return;

        if (isPaused)
            Resume();
        else if (Time.timeScale > 0f)   // don't steal ESC from Game Over / Win screens
            Pause();
    }

    // -------------------------------------------------------

    bool EnsureInitialized()
    {
        if (initialized) return overlay != null;

        var doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[PauseMenuUI] UIDocument component is missing.");
            return false;
        }

        var root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[PauseMenuUI] rootVisualElement is null — is PausePanel.uxml assigned?");
            return false;
        }

        overlay = root.Q<VisualElement>("pause-overlay");

        var btnResume  = root.Q<Button>("btn-resume");
        var btnRestart = root.Q<Button>("btn-restart");
        var btnMenu    = root.Q<Button>("btn-menu");

        if (btnResume  != null) btnResume.clicked  += Resume;
        if (btnRestart != null) btnRestart.clicked += OnRestart;
        if (btnMenu    != null) btnMenu.clicked    += OnMenu;

        if (overlay == null)
            Debug.LogError("[PauseMenuUI] 'pause-overlay' not found — check PausePanel.uxml.");

        initialized = true;
        return overlay != null;
    }

    // -------------------------------------------------------

    void Pause()
    {
        if (!EnsureInitialized()) return;

        isPaused = true;
        overlay.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    void Resume()
    {
        if (OxygenSystem.IsGameOver) return;

        if (overlay != null)
            overlay.style.display = DisplayStyle.None;

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    void OnRestart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneFader.FadeAndLoad(SceneManager.GetActiveScene().name);
    }

    void OnMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneFader.FadeAndLoad("MainMenu");
    }
}
