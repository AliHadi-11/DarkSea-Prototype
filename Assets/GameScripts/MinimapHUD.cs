using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class MinimapHUD : MonoBehaviour
{
    public RenderTexture minimapRT;

    Transform     _player;

    VisualElement _mapView;
    VisualElement _playerArrow;
    VisualElement _sonarRing;

    readonly List<(Transform t, VisualElement dot)> _enemyBlips = new();
    float _enemyScanTimer;

    bool  _sonarActive;
    float _sonarRingTimer;
    const float RING_DURATION  = 1.2f;
    const float MAP_WORLD_SIZE = 20f;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _mapView     = root.Q<VisualElement>("minimap-view");
        _playerArrow = root.Q<VisualElement>("player-arrow");
        _sonarRing   = root.Q<VisualElement>("sonar-ring");

        if (_mapView == null) { Debug.LogError("[MinimapHUD] minimap-view not found."); return; }

        if (minimapRT != null)
            _mapView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(minimapRT));

        if (_playerArrow != null)
            _playerArrow.generateVisualContent += DrawPlayerArrow;

        _player = GameObject.FindWithTag("Player")?.transform;

        SonarSystem.OnPinged += OnSonarPinged;
        RefreshEnemyBlips();
    }

    void OnDisable()
    {
        SonarSystem.OnPinged -= OnSonarPinged;
        foreach (var (_, dot) in _enemyBlips) dot?.RemoveFromHierarchy();
        _enemyBlips.Clear();
    }

    void Update()
    {
        // Player arrow direction
        if (_playerArrow != null && _player != null)
        {
            float yAngle = _player.eulerAngles.y;
            _playerArrow.style.rotate = new StyleRotate(new Rotate(new Angle(yAngle, AngleUnit.Degree)));
            _playerArrow.MarkDirtyRepaint();
        }

        // Enemy blip scan every 1 second
        _enemyScanTimer -= Time.unscaledDeltaTime;
        if (_enemyScanTimer <= 0f) { _enemyScanTimer = 1f; RefreshEnemyBlips(); }
        PositionEnemyBlips();

        // Sonar ring expansion + fade
        if (_sonarActive)
        {
            _sonarRingTimer -= Time.unscaledDeltaTime;
            float t    = 1f - Mathf.Clamp01(_sonarRingTimer / RING_DURATION);
            float size = Mathf.Lerp(10f, 190f, Mathf.SmoothStep(0f, 1f, t));
            float half = size * 0.5f;
            float alpha = Mathf.Clamp01(1.3f - t * 1.5f);

            _sonarRing.style.width    = size;
            _sonarRing.style.height   = size;
            _sonarRing.style.left     = 95f - half;
            _sonarRing.style.top      = 95f - half;
            _sonarRing.style.borderTopLeftRadius     = half;
            _sonarRing.style.borderTopRightRadius    = half;
            _sonarRing.style.borderBottomLeftRadius  = half;
            _sonarRing.style.borderBottomRightRadius = half;
            _sonarRing.style.opacity  = alpha;

            if (_sonarRingTimer <= 0f)
            {
                _sonarActive = false;
                _sonarRing.style.opacity = 0f;
            }
        }
    }

    // ─── Enemy blips ─────────────────────────────────────────────

    void RefreshEnemyBlips()
    {
        foreach (var (_, dot) in _enemyBlips) dot?.RemoveFromHierarchy();
        _enemyBlips.Clear();
        if (_mapView == null) return;

        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            var dot = new VisualElement();
            dot.AddToClassList("enemy-blip");
            _mapView.Add(dot);
            _enemyBlips.Add((e.transform, dot));
        }
    }

    void PositionEnemyBlips()
    {
        if (_player == null) return;
        const float HALF = 95f, BLIP = 6f;
        foreach (var (t, dot) in _enemyBlips)
        {
            if (t == null) { dot.style.display = DisplayStyle.None; continue; }
            Vector3 d = t.position - _player.position;
            dot.style.left = HALF + Mathf.Clamp(d.x / MAP_WORLD_SIZE, -1f, 1f) * HALF * 0.88f - BLIP * 0.5f;
            dot.style.top  = HALF - Mathf.Clamp(d.z / MAP_WORLD_SIZE, -1f, 1f) * HALF * 0.88f - BLIP * 0.5f;
        }
    }

    // ─── Sonar ping ───────────────────────────────────────────────

    void OnSonarPinged(Transform enemy, float enemyDist, bool hasWall, float wallDist)
    {
        _sonarActive    = true;
        _sonarRingTimer = RING_DURATION;
    }

    // ─── Player direction arrow (Painter2D triangle) ──────────────

    void DrawPlayerArrow(MeshGenerationContext mgc)
    {
        var  painter = mgc.painter2D;
        Rect rect    = mgc.visualElement.contentRect;
        float cx = rect.width  * 0.5f;
        float cy = rect.height * 0.5f;
        float h  = rect.height * 0.5f;
        float w  = rect.width  * 0.35f;

        painter.BeginPath();
        painter.MoveTo(new Vector2(cx,     cy - h));   // forward tip
        painter.LineTo(new Vector2(cx + w, cy + h));   // right base
        painter.LineTo(new Vector2(cx - w, cy + h));   // left base
        painter.ClosePath();
        painter.fillColor = new Color(0.58f, 0.80f, 0.90f, 0.90f);
        painter.Fill();
    }
}
