using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Press F to enter/exit the submarine.
// While inside: diver body hides, submarine appears, movement speed increases.
// HUD indicator shown bottom-left when submarine is active.
// Auto-spawns in gameplay scenes — no scene setup required.
public class SubmarineToggle : MonoBehaviour
{
    const KeyCode TOGGLE_KEY = KeyCode.F;
    const float   SPEED_MULT = 1.55f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterCallback()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.Contains("Level") && !scene.name.Contains("DarkSea")) return;
        new GameObject("SubmarineToggle").AddComponent<SubmarineToggle>();
    }

    // ── State ──────────────────────────────────────────────────────────────────
    bool _ready      = false;
    bool _subActive  = false;
    GameObject _subRoot;
    GameObject _playerBody;
    PlayerMovementFinal _movement;
    OxygenSystem _oxygen;
    float _baseSpeed;

    // ── HUD ────────────────────────────────────────────────────────────────────
    VisualElement     _hudPanel;
    SubmarineIconElem _iconElem;
    float             _pulseTime;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    void Start() => StartCoroutine(Setup());

    IEnumerator Setup()
    {
        // Retry up to 5 frames so player Start() is guaranteed to have run
        GameObject player = null;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            yield return null;
            player = GameObject.FindWithTag("Player");
            if (player != null) break;
        }

        if (player == null)
        {
            Debug.LogWarning("[SubmarineToggle] Player not found — submarine disabled.");
            Destroy(gameObject);
            yield break;
        }

        _movement = player.GetComponent<PlayerMovementFinal>();
        _oxygen   = player.GetComponent<OxygenSystem>();
        if (_movement != null) _baseSpeed = _movement.moveSpeed;

        // Cache player body — "PlayerBody" (procedural) OR first child Animator (humanoid model)
        _playerBody = player.transform.Find("PlayerBody")?.gameObject;
        if (_playerBody == null)
        {
            foreach (Transform child in player.transform)
            {
                if (child.GetComponent<Animator>() != null)
                { _playerBody = child.gameObject; break; }
            }
        }

        // Build submarine as hidden child of player
        _subRoot = new GameObject("Submarine");
        _subRoot.transform.SetParent(player.transform);
        _subRoot.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        _subRoot.transform.localRotation = Quaternion.identity;
        _subRoot.AddComponent<SubmarineVisuals>();
        _subRoot.SetActive(false);

        _ready = true;

        // HUD setup (independent — failure here won't break toggle)
        StartCoroutine(SetupHUD());
    }

    void Update()
    {
        if (!_ready) return;
        if (_oxygen != null && _oxygen.currentOxygen <= 0f)
        {
            // Kill submarine on death
            if (_subActive) ForceExit();
            return;
        }

        if (Input.GetKeyDown(TOGGLE_KEY)) Toggle();
        if (_subActive && _hudPanel != null) AnimateHUD();
    }

    // ── Toggle ─────────────────────────────────────────────────────────────────

    void Toggle()
    {
        _subActive = !_subActive;

        _subRoot.SetActive(_subActive);
        if (_playerBody != null) _playerBody.SetActive(!_subActive);
        if (_movement != null)
            _movement.moveSpeed = _subActive ? _baseSpeed * SPEED_MULT : _baseSpeed;

        if (_hudPanel != null)
        {
            _hudPanel.style.display = _subActive ? DisplayStyle.Flex : DisplayStyle.None;
            _pulseTime = 0f;
        }
        if (_iconElem != null) _iconElem.MarkDirtyRepaint();

        string msg = _subActive ? "SUBMARINE DEPLOYED  [F to exit]"
                                : "DIVER MODE  [F for submarine]";
        NotificationUI.Show(msg, NotificationUI.NotifType.Warning);
    }

    void ForceExit()
    {
        _subActive = false;
        if (_subRoot != null)    _subRoot.SetActive(false);
        if (_playerBody != null) _playerBody.SetActive(true);
        if (_movement != null)   _movement.moveSpeed = _baseSpeed;
        if (_hudPanel != null)   _hudPanel.style.display = DisplayStyle.None;
        NotificationUI.Show("DIVER MODE  [F for submarine]", NotificationUI.NotifType.Warning);
    }

    // ── HUD animation ──────────────────────────────────────────────────────────

    void AnimateHUD()
    {
        _pulseTime += Time.deltaTime;
        float a    = 0.55f + Mathf.Sin(_pulseTime * 2.6f) * 0.32f;
        var sc     = new StyleColor(new Color(0.20f, 0.75f, 0.58f, a));
        _hudPanel.style.borderTopColor    = sc;
        _hudPanel.style.borderBottomColor = sc;
        _hudPanel.style.borderLeftColor   = sc;
        _hudPanel.style.borderRightColor  = sc;
    }

    // ── HUD setup ──────────────────────────────────────────────────────────────

    IEnumerator SetupHUD()
    {
        // Find any scene UIDocument for its PanelSettings
        UIDocument sceneDoc = null;
        for (int i = 0; i < 3; i++)
        {
            foreach (var d in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
                if (d.visualTreeAsset != null) { sceneDoc = d; break; }
            if (sceneDoc != null) break;
            yield return null;
        }

        if (sceneDoc?.panelSettings == null) yield break;

        var doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = sceneDoc.panelSettings;
        doc.sortingOrder  = 14;

        yield return null;

        var root = doc.rootVisualElement;
        if (root == null) yield break;

        BuildHUD(root);
    }

    void BuildHUD(VisualElement root)
    {
        // ── Outer panel (left-centre) ──────────────────────────────────────────
        _hudPanel = new VisualElement();
        _hudPanel.style.position    = Position.Absolute;
        _hudPanel.style.left        = 18;
        _hudPanel.style.top         = Length.Percent(42);   // vertical mid-screen
        _hudPanel.style.width       = 222;
        _hudPanel.style.height      = 68;
        _hudPanel.style.display     = DisplayStyle.None;

        _hudPanel.style.backgroundColor  = new StyleColor(new Color(0.03f, 0.07f, 0.11f, 0.86f));
        _hudPanel.style.borderTopWidth    = _hudPanel.style.borderBottomWidth =
        _hudPanel.style.borderLeftWidth   = _hudPanel.style.borderRightWidth = 1.5f;
        var borderCol = new StyleColor(new Color(0.20f, 0.75f, 0.58f, 0.70f));
        _hudPanel.style.borderTopColor    = borderCol;
        _hudPanel.style.borderBottomColor = borderCol;
        _hudPanel.style.borderLeftColor   = borderCol;
        _hudPanel.style.borderRightColor  = borderCol;

        _hudPanel.style.flexDirection  = FlexDirection.Row;
        _hudPanel.style.alignItems     = Align.Center;
        _hudPanel.style.paddingLeft    = 10;
        _hudPanel.style.paddingRight   = 10;
        _hudPanel.pickingMode          = PickingMode.Ignore;
        root.Add(_hudPanel);

        // ── Submarine icon ─────────────────────────────────────────────────────
        _iconElem = new SubmarineIconElem();
        _iconElem.style.width      = 78;
        _iconElem.style.height     = 44;
        _iconElem.style.flexShrink = 0;
        _iconElem.pickingMode      = PickingMode.Ignore;
        _hudPanel.Add(_iconElem);

        // ── Text column ────────────────────────────────────────────────────────
        var col = new VisualElement();
        col.style.flexDirection  = FlexDirection.Column;
        col.style.justifyContent = Justify.Center;
        col.style.marginLeft     = 6;
        col.pickingMode          = PickingMode.Ignore;
        _hudPanel.Add(col);

        var title = new Label("SUBMARINE");
        title.style.fontSize                = 14;
        title.style.color                   = new StyleColor(new Color(0.28f, 0.92f, 0.72f));
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.unityTextAlign          = TextAnchor.MiddleLeft;
        title.pickingMode                   = PickingMode.Ignore;
        col.Add(title);

        var sub = new Label("SPD x1.5   |   [F] EXIT");
        sub.style.fontSize       = 9;
        sub.style.color          = new StyleColor(new Color(0.48f, 0.70f, 0.62f));
        sub.style.marginTop      = 3;
        sub.style.unityTextAlign = TextAnchor.MiddleLeft;
        sub.pickingMode          = PickingMode.Ignore;
        col.Add(sub);

        // Active green dot (top-right)
        var dot = new VisualElement();
        dot.style.position                  = Position.Absolute;
        dot.style.top                       = 6;
        dot.style.right                     = 8;
        dot.style.width                     = 7;
        dot.style.height                    = 7;
        dot.style.borderTopLeftRadius       = 4;
        dot.style.borderTopRightRadius      = 4;
        dot.style.borderBottomLeftRadius    = 4;
        dot.style.borderBottomRightRadius   = 4;
        dot.style.backgroundColor          = new StyleColor(new Color(0.12f, 0.92f, 0.50f));
        dot.pickingMode                     = PickingMode.Ignore;
        _hudPanel.Add(dot);
    }

    // ── Submarine icon (Painter2D) ─────────────────────────────────────────────

    class SubmarineIconElem : VisualElement
    {
        public SubmarineIconElem() { generateVisualContent += Draw; }

        void Draw(MeshGenerationContext ctx)
        {
            var p = ctx.painter2D;
            float w = contentRect.width;
            float h = contentRect.height;

            var hull  = new Color(0.22f, 0.82f, 0.64f, 0.95f);
            var tower = new Color(0.18f, 0.68f, 0.52f, 0.95f);
            var glass = new Color(0.06f, 0.38f, 0.32f, 0.90f);
            var amber = new Color(0.92f, 0.54f, 0.08f, 1.00f);

            float hullW = w * 0.82f;
            float hullH = h * 0.35f;
            float hullX = (w - hullW) * 0.5f;
            float cy    = h * 0.60f;
            float hullY = cy - hullH * 0.5f;
            float capR  = hullH * 0.5f;

            // Hull flat body
            p.fillColor = hull;
            Rect(p, hullX + capR, hullY, hullW - capR * 2f, hullH);

            // Front cap
            p.BeginPath();
            p.Arc(new Vector2(hullX + hullW - capR, cy), capR, -90f, 90f);
            p.ClosePath(); p.Fill();

            // Rear cap
            p.BeginPath();
            p.Arc(new Vector2(hullX + capR, cy), capR, 90f, 270f);
            p.ClosePath(); p.Fill();

            // Conning tower
            float twW  = hullW * 0.16f;
            float twH  = h * 0.28f;
            float twCX = w * 0.50f;
            float twX  = twCX - twW * 0.5f;
            float twY  = hullY - twH;
            p.fillColor = tower;
            Rect(p, twX, twY, twW, twH);
            p.BeginPath();
            p.Arc(new Vector2(twCX, twY), twW * 0.5f, 180f, 360f);
            p.ClosePath(); p.Fill();

            // Periscope
            p.fillColor = tower;
            Rect(p, twX + twW * 0.72f, twY - h * 0.15f, 1.5f, h * 0.15f);
            Rect(p, twX + twW * 0.40f, twY - h * 0.15f, 5f, 1.5f);

            // Front viewport
            p.fillColor = glass;
            p.BeginPath();
            p.Arc(new Vector2(hullX + hullW - capR * 0.62f, cy - capR * 0.10f),
                  capR * 0.40f, 0f, 360f);
            p.Fill();

            // Side fins
            p.fillColor = hull;
            Rect(p, hullX + hullW * 0.34f, hullY + hullH - 1.5f, hullW * 0.24f, 4f);
            Rect(p, hullX + hullW * 0.10f, hullY + hullH - 1.5f, hullW * 0.22f, 4f);

            // Propeller blades (3)
            float propX = hullX + capR * 0.50f;
            p.fillColor = amber;
            for (int i = 0; i < 3; i++)
            {
                float ang = i * 120f * Mathf.Deg2Rad + Mathf.PI * 0.5f;
                float bx  = propX + Mathf.Cos(ang) * capR * 0.70f;
                float by  = cy    + Mathf.Sin(ang) * capR * 0.70f;
                float dx  = bx - propX;
                float dy  = by - cy;
                float len = Mathf.Sqrt(dx * dx + dy * dy);
                if (len > 0.5f)
                {
                    float nx = -dy / len * 1.2f;
                    float ny =  dx / len * 1.2f;
                    p.BeginPath();
                    p.MoveTo(new Vector2(propX + nx, cy + ny));
                    p.LineTo(new Vector2(propX - nx, cy - ny));
                    p.LineTo(new Vector2(bx    - nx, by - ny));
                    p.LineTo(new Vector2(bx    + nx, by + ny));
                    p.ClosePath(); p.Fill();
                }
            }
            p.BeginPath();
            p.Arc(new Vector2(propX, cy), 2.2f, 0f, 360f);
            p.Fill();

            // Nav lights
            p.fillColor = new Color(0.12f, 1.00f, 0.32f, 0.95f);
            p.BeginPath();
            p.Arc(new Vector2(hullX + hullW - capR * 1.1f, hullY + hullH * 0.30f), 2f, 0f, 360f);
            p.Fill();
            p.fillColor = new Color(1.00f, 0.14f, 0.08f, 0.95f);
            p.BeginPath();
            p.Arc(new Vector2(hullX + hullW - capR * 1.1f, hullY + hullH * 0.70f), 2f, 0f, 360f);
            p.Fill();

            // Masthead
            p.fillColor = amber;
            p.BeginPath();
            p.Arc(new Vector2(twCX, twY - 1.5f), 2.0f, 0f, 360f);
            p.Fill();
        }

        static void Rect(Painter2D p, float x, float y, float w, float h)
        {
            if (w <= 0f || h <= 0f) return;
            p.BeginPath();
            p.MoveTo(new Vector2(x, y));
            p.LineTo(new Vector2(x + w, y));
            p.LineTo(new Vector2(x + w, y + h));
            p.LineTo(new Vector2(x, y + h));
            p.ClosePath();
            p.Fill();
        }
    }
}
