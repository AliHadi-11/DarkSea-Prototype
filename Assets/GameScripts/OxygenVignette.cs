using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Full-screen red vignette that intensifies and pulses when oxygen is critically low.
// Auto-spawns in gameplay scenes — no scene setup required.
public class OxygenVignette : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.Contains("Level") && !scene.Contains("DarkSea")) return;
        new GameObject("OxygenVignette").AddComponent<OxygenVignette>();
    }

    UIDocument   _doc;
    VignetteElem _vignette;
    OxygenSystem _oxygenSystem;
    float        _currentIntensity;

    void Start()
    {
        var existingDoc = Object.FindFirstObjectByType<UIDocument>();
        if (existingDoc == null || existingDoc.panelSettings == null) return;

        _doc = gameObject.AddComponent<UIDocument>();
        _doc.panelSettings = existingDoc.panelSettings;
        _doc.sortingOrder  = 30; // above HUD (10) below WinPanel (50)

        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        yield return null; // wait one frame for rootVisualElement

        var root = _doc?.rootVisualElement;
        if (root == null) yield break;

        _vignette = new VignetteElem();
        root.Add(_vignette);

        _oxygenSystem = Object.FindFirstObjectByType<OxygenSystem>();
    }

    void Update()
    {
        if (_vignette == null || _oxygenSystem == null) return;

        float o = _oxygenSystem.currentOxygen;
        float target = 0f;

        if (o < 30f)
        {
            float t = (30f - Mathf.Max(o, 0f)) / 30f; // 0 at 30%, 1 at 0%
            target = t * t; // quadratic ramp
        }

        // Pulse at critical oxygen
        if (o < 20f)
        {
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 2.8f));
            target *= 0.55f + 0.45f * pulse;
        }

        _currentIntensity = Mathf.MoveTowards(_currentIntensity, target, Time.deltaTime * 1.8f);
        _vignette.SetIntensity(_currentIntensity);
    }

    // ── Inner VisualElement ──────────────────────────────────────────────────

    class VignetteElem : VisualElement
    {
        float _intensity;

        public VignetteElem()
        {
            style.position = Position.Absolute;
            style.left = 0; style.right  = 0;
            style.top  = 0; style.bottom = 0;
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public void SetIntensity(float v)
        {
            if (Mathf.Abs(v - _intensity) < 0.005f) return;
            _intensity = v;
            MarkDirtyRepaint();
        }

        void Draw(MeshGenerationContext ctx)
        {
            if (_intensity < 0.005f) return;

            var p = ctx.painter2D;
            var r = contentRect;

            // Stack 10 concentric frames (outermost = most opaque → transparent at center).
            float maxInset = Mathf.Min(r.width, r.height) * 0.40f;
            int   layers   = 10;

            for (int i = 0; i < layers; i++)
            {
                float t     = i / (float)(layers - 1);
                float inset = maxInset * t;
                float alpha = _intensity * (1f - t) * 0.55f;
                if (alpha < 0.004f) continue;

                float barH = maxInset / layers + 2f;

                p.fillColor = new Color(0.75f, 0f, 0f, alpha);

                // top bar
                FilledRect(p, inset, inset, r.width - inset * 2f, barH);

                // bottom bar
                FilledRect(p, inset, r.height - inset - barH, r.width - inset * 2f, barH);

                // left bar
                FilledRect(p, inset, inset + barH, barH, r.height - (inset + barH) * 2f);

                // right bar
                FilledRect(p, r.width - inset - barH, inset + barH, barH, r.height - (inset + barH) * 2f);
            }
        }

        // Painter2D has no Rect() — draw a filled quad via MoveTo/LineTo.
        static void FilledRect(Painter2D p, float x, float y, float w, float h)
        {
            if (w <= 0f || h <= 0f) return;
            p.BeginPath();
            p.MoveTo(new Vector2(x,     y));
            p.LineTo(new Vector2(x + w, y));
            p.LineTo(new Vector2(x + w, y + h));
            p.LineTo(new Vector2(x,     y + h));
            p.ClosePath();
            p.Fill();
        }
    }
}
