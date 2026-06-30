using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Adds a level timer, enemy count, and objective line to gameplay scenes.
// Auto-spawns — no scene setup required.
public class GameHUDExtras : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.Contains("Level") && !scene.Contains("DarkSea")) return;
        new GameObject("GameHUDExtras").AddComponent<GameHUDExtras>();
    }

    Label  _timerLabel;
    Label  _enemyLabel;
    Label  _objLabel;
    float  _elapsed;
    float  _enemyRefreshTimer;
    bool   _isLevel2;

    static readonly Color TEXT_DIM  = new Color(0.55f, 0.68f, 0.60f, 1f);
    static readonly Color TEXT_RED  = new Color(0.85f, 0.25f, 0.22f, 1f);
    static readonly Color TEXT_AMB  = new Color(0.745f, 0.60f, 0.35f, 1f);

    void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        _isLevel2 = scene.Contains("Level_2");

        string obj = _isLevel2
            ? "COLLECT OXYGEN TANKS ─ ELIMINATE ENEMIES"
            : scene.Contains("Level_3")
                ? "SURVIVE THE MIMIC ─ REACH THE EXIT"
                : "SURVIVE ─ REACH THE EXIT";

        StartCoroutine(Setup(obj));
    }

    System.Collections.IEnumerator Setup(string objective)
    {
        // Find scene UIDocument (one with an assigned UXML asset)
        UIDocument sceneDoc = null;
        foreach (var d in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
        {
            if (d.visualTreeAsset != null) { sceneDoc = d; break; }
        }
        if (sceneDoc == null || sceneDoc.panelSettings == null) yield break;

        var doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = sceneDoc.panelSettings;
        doc.sortingOrder  = 12; // just above AdvancedHUD (10)

        yield return null;

        var root = doc.rootVisualElement;
        if (root == null) yield break;

        // ── Timer — top-right ──────────────────────────────────────────
        _timerLabel = Lbl("0:00", 15, TEXT_DIM, FontStyle.Bold);
        _timerLabel.style.position = Position.Absolute;
        _timerLabel.style.top   = 14;
        _timerLabel.style.right = 16;
        root.Add(_timerLabel);

        // ── Enemy count — below timer (Level 2 + 3) ───────────────────
        _enemyLabel = Lbl("ENEMIES: —", 12, TEXT_RED, FontStyle.Normal);
        _enemyLabel.style.position = Position.Absolute;
        _enemyLabel.style.top   = 36;
        _enemyLabel.style.right = 16;
        _enemyLabel.style.display = _isLevel2 ? DisplayStyle.Flex : DisplayStyle.None;
        root.Add(_enemyLabel);

        // ── Objective — bottom-center ──────────────────────────────────
        _objLabel = Lbl(objective, 11, TEXT_AMB, FontStyle.Normal);
        _objLabel.style.position = Position.Absolute;
        _objLabel.style.bottom = 10;
        _objLabel.style.left   = 0;
        _objLabel.style.right  = 0;
        _objLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _objLabel.pickingMode = PickingMode.Ignore;
        root.Add(_objLabel);
    }

    void Update()
    {
        _elapsed += Time.deltaTime;

        if (_timerLabel != null)
        {
            int mins = (int)(_elapsed / 60f);
            int secs = (int)(_elapsed % 60f);
            _timerLabel.text = mins + ":" + secs.ToString("D2");
        }

        if (_isLevel2 && _enemyLabel != null)
        {
            _enemyRefreshTimer -= Time.deltaTime;
            if (_enemyRefreshTimer <= 0f)
            {
                _enemyRefreshTimer = 0.5f;
                int count = GameObject.FindGameObjectsWithTag("Enemy").Length;
                _enemyLabel.text = "ENEMIES: " + count;
                _enemyLabel.style.color = new StyleColor(count > 0 ? TEXT_RED : TEXT_DIM);
            }
        }
    }

    static Label Lbl(string text, int size, Color color, FontStyle style)
    {
        var l = new Label(text);
        l.style.fontSize = size;
        l.style.color    = new StyleColor(color);
        l.style.unityFontStyleAndWeight = style;
        l.pickingMode = PickingMode.Ignore;
        return l;
    }
}
