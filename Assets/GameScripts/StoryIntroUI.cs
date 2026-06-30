using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

// Shows a typewriter story intro once per game session (resets when exe is reopened).
// Auto-spawns in the first scene — no setup required.
// Press any key to skip typewriter; press again to dismiss.
public class StoryIntroUI : MonoBehaviour
{
    static bool _shownThisSession = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        if (_shownThisSession) return;
        new GameObject("StoryIntroUI").AddComponent<StoryIntroUI>();
    }

    // ── Story text ────────────────────────────────────────────────────
    static readonly string[] LINES =
    {
        "YEAR  2157.",
        "",
        "The deep-sea oxygen reserves —",
        "humanity's last lifeline — have gone dark.",
        "",
        "All contact with Station MERIDIAN lost.",
        "Crew status: unknown.",
        "",
        "You are  DIVER-7.",
        "Your mission: reach the reserves,",
        "restore oxygen flow,  survive the dark.",
    };

    // ── State ─────────────────────────────────────────────────────────
    VisualElement _overlay;
    Label         _textLabel;
    Label         _promptLabel;
    bool          _inputReady;
    bool          _skipTyping;

    // ── Audio ──────────────────────────────────────────────────────────
    AudioSource _typeSrc;
    AudioClip   _typeClip;

    // ── Bootstrap ─────────────────────────────────────────────────────

    void Start() => StartCoroutine(Setup());

    IEnumerator Setup()
    {
        UIDocument sceneDoc = null;
        foreach (var d in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            if (d.visualTreeAsset != null) { sceneDoc = d; break; }

        if (sceneDoc?.panelSettings == null) { Destroy(gameObject); yield break; }

        var doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = sceneDoc.panelSettings;
        doc.sortingOrder  = 200;

        // Audio setup
        _typeSrc            = gameObject.AddComponent<AudioSource>();
        _typeSrc.spatialBlend = 0f;
        _typeSrc.playOnAwake  = false;
        _typeSrc.volume       = 0.28f;
        // Try user-provided clip first, else generate procedural click
        _typeClip = Resources.Load<AudioClip>("Audio/typewriter");
        if (_typeClip == null) _typeClip = GenerateTypeClick();

        yield return null;

        var root = doc.rootVisualElement;
        if (root == null) { Destroy(gameObject); yield break; }

        BuildUI(root);
        _shownThisSession = true;
        StartCoroutine(TypewriterSequence());
    }

    // Procedural typewriter click: short noise burst with fast decay
    static AudioClip GenerateTypeClick()
    {
        const int   sampleRate = 44100;
        const float duration   = 0.030f;
        int         len        = (int)(sampleRate * duration);
        var         clip       = AudioClip.Create("TypeClick", len, 1, sampleRate, false);
        var         data       = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t       = i / (float)len;
            float envelope = Mathf.Exp(-t * 38f);
            data[i] = (Random.value * 2f - 1f) * envelope;
        }
        clip.SetData(data, 0);
        return clip;
    }

    void PlayTypeClick()
    {
        if (_typeSrc == null || _typeClip == null) return;
        _typeSrc.pitch = Random.Range(0.88f, 1.12f);
        _typeSrc.PlayOneShot(_typeClip, Random.Range(0.22f, 0.32f));
    }

    void BuildUI(VisualElement root)
    {
        // Full-screen dark backdrop
        _overlay = new VisualElement();
        _overlay.style.position = Position.Absolute;
        _overlay.style.left = 0; _overlay.style.right  = 0;
        _overlay.style.top  = 0; _overlay.style.bottom = 0;
        _overlay.style.backgroundColor = new StyleColor(new Color(0.012f, 0.020f, 0.016f, 1f));
        _overlay.style.alignItems     = Align.Center;
        _overlay.style.justifyContent = Justify.Center;
        _overlay.pickingMode = PickingMode.Position;
        root.Add(_overlay);

        // Story text — centered, ~60% width
        _textLabel = new Label();
        _textLabel.style.fontSize    = 17;
        _textLabel.style.color       = new StyleColor(new Color(0.52f, 0.74f, 0.63f));
        _textLabel.style.whiteSpace  = WhiteSpace.Normal;
        _textLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _textLabel.style.width       = Length.Percent(62);
        _textLabel.style.marginBottom= 50;
        _textLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
        _textLabel.pickingMode = PickingMode.Ignore;
        _overlay.Add(_textLabel);

        // Skip hint — small, top-right
        var skipHint = new Label("PRESS ANY KEY TO SKIP");
        skipHint.style.position  = Position.Absolute;
        skipHint.style.top       = 18;
        skipHint.style.right     = 22;
        skipHint.style.fontSize  = 10;
        skipHint.style.color     = new StyleColor(new Color(0.28f, 0.40f, 0.34f));
        skipHint.pickingMode = PickingMode.Ignore;
        _overlay.Add(skipHint);

        // Press-any-key prompt — bottom-center, hidden until text done
        _promptLabel = new Label("[ PRESS ANY KEY TO CONTINUE ]");
        _promptLabel.style.position = Position.Absolute;
        _promptLabel.style.bottom   = 36;
        _promptLabel.style.left = 0; _promptLabel.style.right = 0;
        _promptLabel.style.fontSize = 12;
        _promptLabel.style.color    = new StyleColor(new Color(0.38f, 0.56f, 0.48f));
        _promptLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _promptLabel.style.display  = DisplayStyle.None;
        _promptLabel.pickingMode    = PickingMode.Ignore;
        _overlay.Add(_promptLabel);
    }

    // ── Typewriter ────────────────────────────────────────────────────

    IEnumerator TypewriterSequence()
    {
        yield return new WaitForSecondsRealtime(0.6f);

        var sb = new StringBuilder();

        foreach (string line in LINES)
        {
            if (string.IsNullOrEmpty(line))
            {
                sb.Append('\n');
                if (_textLabel != null) _textLabel.text = sb.ToString();
                if (!_skipTyping) yield return new WaitForSecondsRealtime(0.30f);
                continue;
            }

            foreach (char c in line)
            {
                if (_skipTyping) break;
                sb.Append(c);
                if (_textLabel != null) _textLabel.text = sb.ToString();
                // Play click for visible characters only (skip spaces silently)
                if (c != ' ') PlayTypeClick();
                yield return new WaitForSecondsRealtime(0.028f);
            }

            if (_skipTyping) break;
            sb.Append('\n');
            if (_textLabel != null) _textLabel.text = sb.ToString();
            yield return new WaitForSecondsRealtime(0.28f);
        }

        // Show complete text instantly if player skipped
        if (_textLabel != null)
            _textLabel.text = string.Join("\n", LINES);

        if (_promptLabel != null)
            _promptLabel.style.display = DisplayStyle.Flex;

        _inputReady = true;
        StartCoroutine(BlinkPrompt());
    }

    IEnumerator BlinkPrompt()
    {
        while (_inputReady && _promptLabel != null)
        {
            _promptLabel.style.opacity = 1f;
            yield return new WaitForSecondsRealtime(0.55f);
            _promptLabel.style.opacity = 0.15f;
            yield return new WaitForSecondsRealtime(0.35f);
        }
    }

    // ── Input ─────────────────────────────────────────────────────────

    void Update()
    {
        if (!Input.anyKeyDown) return;

        if (!_inputReady)
        {
            _skipTyping = true; // jump to full text
        }
        else
        {
            StartCoroutine(FadeOutAndDone());
        }
    }

    IEnumerator FadeOutAndDone()
    {
        _inputReady = false;
        float t = 0f;
        while (t < 0.45f && _overlay != null)
        {
            t += Time.unscaledDeltaTime;
            _overlay.style.opacity = 1f - Mathf.Clamp01(t / 0.45f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
