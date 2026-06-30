using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Black fade-in on every scene load. Provides SceneFader.FadeAndLoad(sceneName)
// for smooth fade-out then scene transition. Auto-spawns — no scene setup required.
public class SceneFader : MonoBehaviour
{
    const float FADE_SEC = 0.35f;

    static SceneFader _instance;
    void OnDestroy() { if (_instance == this) _instance = null; }

    UIDocument   _doc;
    VisualElement _overlay;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        if (_instance != null) return;
        _instance = new GameObject("SceneFader").AddComponent<SceneFader>();
    }

    void Start()
    {
        var existingDoc = Object.FindFirstObjectByType<UIDocument>();
        if (existingDoc == null || existingDoc.panelSettings == null) return;

        _doc = gameObject.AddComponent<UIDocument>();
        _doc.panelSettings = existingDoc.panelSettings;
        _doc.sortingOrder  = 100; // top of everything

        StartCoroutine(SetupAndFadeIn());
    }

    IEnumerator SetupAndFadeIn()
    {
        yield return null; // wait for rootVisualElement

        var root = _doc?.rootVisualElement;
        if (root == null) yield break;

        _overlay = new VisualElement();
        _overlay.style.position = Position.Absolute;
        _overlay.style.left   = 0; _overlay.style.right  = 0;
        _overlay.style.top    = 0; _overlay.style.bottom = 0;
        _overlay.style.backgroundColor = new StyleColor(Color.black);
        _overlay.pickingMode = PickingMode.Ignore;
        root.Add(_overlay);

        // Fade from black
        float t = 0f;
        while (t < FADE_SEC)
        {
            t += Time.unscaledDeltaTime;
            float a = 1f - Mathf.Clamp01(t / FADE_SEC);
            _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, a));
            yield return null;
        }
        _overlay.style.display = DisplayStyle.None;
    }

    // Replace SceneManager.LoadScene calls with this for smooth transitions.
    public static void FadeAndLoad(string sceneName)
    {
        if (_instance != null)
            _instance.StartCoroutine(_instance.FadeOutAndLoad(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        if (_overlay != null)
        {
            _overlay.style.display = DisplayStyle.Flex;
            _overlay.pickingMode   = PickingMode.Position; // block input during fade

            float t = 0f;
            while (t < FADE_SEC)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / FADE_SEC);
                _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, a));
                yield return null;
            }
        }
        SceneManager.LoadScene(sceneName);
    }
}
