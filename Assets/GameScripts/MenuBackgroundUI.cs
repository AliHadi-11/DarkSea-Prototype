using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Animated bubble background for the Main Menu scene.
// Auto-spawns — no scene setup required.
// Inserts itself as the first child of MainMenuUI's root so it renders behind all menu elements.
public class MenuBackgroundUI : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu") return;
        new GameObject("MenuBackgroundUI").AddComponent<MenuBackgroundUI>();
    }

    BubbleCanvas _canvas;

    void Start() => StartCoroutine(Setup());

    IEnumerator Setup()
    {
        // Wait one frame so MainMenuUI's OnEnable has run
        yield return null;

        var mainMenuUI = Object.FindFirstObjectByType<MainMenuUI>();
        if (mainMenuUI == null) yield break;

        var doc  = mainMenuUI.GetComponent<UIDocument>();
        var root = doc?.rootVisualElement;
        if (root == null) yield break;

        _canvas = new BubbleCanvas(32);
        _canvas.style.position = Position.Absolute;
        _canvas.style.left = 0; _canvas.style.right  = 0;
        _canvas.style.top  = 0; _canvas.style.bottom = 0;
        _canvas.pickingMode = PickingMode.Ignore;

        root.Insert(0, _canvas); // index 0 = behind every other child
    }

    void Update()
    {
        _canvas?.Tick(Time.unscaledDeltaTime);
    }

    // ── Bubble Canvas ─────────────────────────────────────────────────

    class BubbleCanvas : VisualElement
    {
        struct Bubble
        {
            public float x, y;       // position (pixels, top-left origin)
            public float radius;
            public float speedY;     // upward speed px/s
            public float driftX;     // horizontal drift px/s
            public float alpha;
        }

        readonly Bubble[] _bubbles;
        float _w, _h;

        public BubbleCanvas(int count)
        {
            _bubbles = new Bubble[count];
            generateVisualContent += Draw;

            // Initialise with dummy size — will re-seed when first size is known
            for (int i = 0; i < count; i++)
                _bubbles[i] = MakeBubble(Random.Range(0f, 800f), Random.Range(0f, 600f), 800f, 600f);
        }

        public void Tick(float dt)
        {
            float w = resolvedStyle.width;
            float h = resolvedStyle.height;

            if (w < 10f || h < 10f) { MarkDirtyRepaint(); return; }

            // Re-seed if screen size just became known or changed
            if (Mathf.Abs(w - _w) > 1f || Mathf.Abs(h - _h) > 1f)
            {
                _w = w; _h = h;
                for (int i = 0; i < _bubbles.Length; i++)
                    _bubbles[i] = MakeBubble(Random.Range(0f, w), Random.Range(0f, h), w, h);
            }

            for (int i = 0; i < _bubbles.Length; i++)
            {
                ref Bubble b = ref _bubbles[i];
                b.y -= b.speedY * dt;     // move upward (y=0 is top)
                b.x += b.driftX * dt;

                // Wrap top → bottom
                if (b.y + b.radius < 0f)
                    b = MakeBubble(Random.Range(0f, _w), _h + b.radius, _w, _h);

                // Wrap horizontally
                if (b.x < -b.radius)      b.x = _w + b.radius;
                if (b.x > _w + b.radius)  b.x = -b.radius;
            }

            MarkDirtyRepaint();
        }

        void Draw(MeshGenerationContext ctx)
        {
            var p = ctx.painter2D;

            // Subtle dark corners — four corner vignettes
            DrawCornerVignette(p, contentRect);

            // Bubbles
            foreach (var b in _bubbles)
            {
                // Outer glow
                p.fillColor = new Color(0.18f, 0.52f, 0.42f, b.alpha * 0.35f);
                p.BeginPath();
                p.Arc(new Vector2(b.x, b.y), b.radius * 1.7f, 0f, 360f);
                p.Fill();

                // Core bubble
                p.fillColor = new Color(0.22f, 0.60f, 0.48f, b.alpha);
                p.BeginPath();
                p.Arc(new Vector2(b.x, b.y), b.radius, 0f, 360f);
                p.Fill();

                // Tiny highlight (top-left of bubble)
                p.fillColor = new Color(0.55f, 0.85f, 0.75f, b.alpha * 0.55f);
                p.BeginPath();
                p.Arc(new Vector2(b.x - b.radius * 0.28f, b.y - b.radius * 0.28f),
                      b.radius * 0.28f, 0f, 360f);
                p.Fill();
            }
        }

        static void DrawCornerVignette(Painter2D p, Rect r)
        {
            // Draw 6 concentric frames, progressively smaller, at low alpha
            // to give a subtle "underwater darkness at edges" feel.
            int layers = 6;
            float maxInset = Mathf.Min(r.width, r.height) * 0.22f;

            for (int i = 0; i < layers; i++)
            {
                float t     = i / (float)(layers - 1);
                float inset = maxInset * t;
                float alpha = (1f - t) * 0.18f;
                float barH  = maxInset / layers + 2f;

                p.fillColor = new Color(0.01f, 0.03f, 0.02f, alpha);

                // top
                DrawRect(p, inset, inset, r.width - inset * 2f, barH);
                // bottom
                DrawRect(p, inset, r.height - inset - barH, r.width - inset * 2f, barH);
                // left
                DrawRect(p, inset, inset + barH, barH, r.height - (inset + barH) * 2f);
                // right
                DrawRect(p, r.width - inset - barH, inset + barH, barH, r.height - (inset + barH) * 2f);
            }
        }

        static void DrawRect(Painter2D p, float x, float y, float w, float h)
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

        static Bubble MakeBubble(float x, float y, float w, float h)
        {
            float r = Random.Range(2.5f, 7.5f);
            return new Bubble
            {
                x      = x,
                y      = y,
                radius = r,
                speedY = Random.Range(18f, 52f),
                driftX = Random.Range(-6f, 6f),
                alpha  = Random.Range(0.07f, 0.22f),
            };
        }
    }
}
