using UnityEngine;
using UnityEngine.UIElements;

// =========================================================
//  Dark Sea — Advanced HUD Manager
//
//  Provides:
//   • Oxygen bar with color transitions (cyan→orange→red)
//     and a pulsing critical state below 20%
//   • Sonar radar drawn with Painter2D:
//     - range rings, crosshair
//     - green sweep sector when Space is pressed
//     - red enemy blip (fades after 4 s)
//     - blue wall indicator (forward direction)
//
//  Setup (done automatically by scene setup script):
//   • Put this on a GameObject that also has UIDocument
//   • UIDocument.sourceAsset = HUD.uxml
//   • Needs OxygenSystem and SonarSystem in the scene
//   • SonarSystem.OnPinged event is used for radar updates
// =========================================================
[RequireComponent(typeof(UIDocument))]
public class AdvancedHUD : MonoBehaviour
{
    // ── Runtime scene references ──────────────────────────
    OxygenSystem _oxygen;
    Transform    _player;

    // ── UI elements ───────────────────────────────────────
    VisualElement _fill;
    Label         _pctLabel;
    Label         _sonarLog;
    VisualElement _radarEl;   // procedurally drawn

    // ── Oxygen state ──────────────────────────────────────
    IVisualElementScheduledItem _pulseSchedule;
    bool _pulseDim;
    bool _wasCritical;

    // ── Sonar / radar state ───────────────────────────────
    float _sweepAngle;
    float _sweepTimer;
    bool  _isSweeping;

    bool    _hasEnemyBlip;
    float   _enemyBlipTimer;
    Vector2 _enemyBlipLocal; // normalized (-1..1) in radar space

    bool  _hasWallBlip;

    const float SWEEP_DURATION = 1.5f;
    const float BLIP_FADE      = 4.0f;

    // ─────────────────────────────────────────────────────

    void OnEnable()
    {
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) { Debug.LogError("[AdvancedHUD] rootVisualElement null."); return; }

        _fill     = root.Q<VisualElement>("oxygen-bar-fill");
        _pctLabel = root.Q<Label>("oxygen-pct");
        _sonarLog = root.Q<Label>("sonar-log");

        // Build the procedural radar element and insert into the container
        var radarContainer = root.Q<VisualElement>("sonar-radar-container");
        _radarEl = new VisualElement();
        _radarEl.style.position = Position.Absolute;
        _radarEl.style.left     = 0;
        _radarEl.style.top      = 0;
        _radarEl.style.right    = 0;
        _radarEl.style.bottom   = 0;
        _radarEl.generateVisualContent += DrawRadar;
        radarContainer?.Add(_radarEl);

        // Find scene systems
        _oxygen = Object.FindFirstObjectByType<OxygenSystem>();
        _player = GameObject.FindWithTag("Player")?.transform;

        if (_oxygen == null) Debug.LogWarning("[AdvancedHUD] OxygenSystem not found.");
        if (_player == null) Debug.LogWarning("[AdvancedHUD] Player tag not found.");

        // Subscribe to sonar pings
        SonarSystem.OnPinged += OnSonarPinged;
    }

    void OnDisable()
    {
        SonarSystem.OnPinged -= OnSonarPinged;
        _pulseSchedule?.Pause();
    }

    // ── Update: oxygen bar + radar repaint ───────────────

    void Update()
    {
        // Oxygen
        if (_oxygen != null)
        {
            float pct = Mathf.Clamp01(_oxygen.currentOxygen / 100f);
            UpdateOxygenBar(pct);
        }

        // Sweep animation
        if (_isSweeping)
        {
            _sweepTimer += Time.unscaledDeltaTime;
            _sweepAngle  = (_sweepTimer / SWEEP_DURATION) * 360f;
            if (_sweepTimer >= SWEEP_DURATION)
            {
                _isSweeping = false;
                _sweepAngle = 0f;
            }
        }

        // Enemy blip fade
        if (_hasEnemyBlip)
        {
            _enemyBlipTimer -= Time.unscaledDeltaTime;
            if (_enemyBlipTimer <= 0f) _hasEnemyBlip = false;
        }

        _radarEl?.MarkDirtyRepaint();
    }

    // ── Oxygen bar ────────────────────────────────────────

    void UpdateOxygenBar(float t)
    {
        if (_fill == null) return;

        _fill.style.width = Length.Percent(t * 100f);

        // Clear old color classes
        _fill.RemoveFromClassList("oxygen-fill-mid");
        _fill.RemoveFromClassList("oxygen-fill-low");
        _fill.RemoveFromClassList("oxygen-fill-critical");

        bool isCritical = t < 0.2f;

        if (t >= 0.5f)
        {
            // default cyan — no extra class
            _pctLabel?.RemoveFromClassList("oxygen-pct-low");
        }
        else if (t >= 0.2f)
        {
            _fill.AddToClassList("oxygen-fill-mid");
            _pctLabel?.RemoveFromClassList("oxygen-pct-low");
        }
        else
        {
            _fill.AddToClassList("oxygen-fill-critical");
            _pctLabel?.AddToClassList("oxygen-pct-low");
        }

        // Start/stop pulse
        if (isCritical && !_wasCritical)
            StartCriticalPulse();
        else if (!isCritical && _wasCritical)
            StopCriticalPulse();
        _wasCritical = isCritical;

        if (_pctLabel != null)
            _pctLabel.text = Mathf.CeilToInt(t * 100f) + "%";
    }

    void StartCriticalPulse()
    {
        _pulseSchedule?.Pause();
        _pulseSchedule = _fill?.schedule
            .Execute(() =>
            {
                _pulseDim = !_pulseDim;
                if (_pulseDim) _fill.AddToClassList("oxygen-fill-pulse-dim");
                else           _fill.RemoveFromClassList("oxygen-fill-pulse-dim");
            })
            .Every(300)
            .StartingIn(0);
    }

    void StopCriticalPulse()
    {
        _pulseSchedule?.Pause();
        _fill?.RemoveFromClassList("oxygen-fill-pulse-dim");
        _fill?.RemoveFromClassList("oxygen-fill-critical");
    }

    // ── Sonar event ───────────────────────────────────────

    void OnSonarPinged(Transform enemy, float enemyDist, bool hasWall, float wallDist)
    {
        // Start sweep animation
        _isSweeping = true;
        _sweepTimer = 0f;
        _sweepAngle = 0f;

        // Enemy blip
        if (enemy != null && _player != null)
        {
            Vector3 worldOffset = enemy.position - _player.position;
            Vector3 localOffset = _player.InverseTransformDirection(worldOffset);
            float maxRange = 14f;
            _enemyBlipLocal = new Vector2(
                Mathf.Clamp(localOffset.x / maxRange, -0.95f, 0.95f),
                Mathf.Clamp(localOffset.z / maxRange, -0.95f, 0.95f)
            );
            _hasEnemyBlip   = true;
            _enemyBlipTimer = BLIP_FADE;
        }
        else
        {
            _hasEnemyBlip = false;
        }

        _hasWallBlip = hasWall;

        // Update text log
        if (_sonarLog != null)
        {
            string txt = hasWall ? "WALL: " + (int)wallDist + "m" : "CLEAR";
            if (enemy != null) txt += "  |  ENEMY: " + (int)enemyDist + "m";
            _sonarLog.text = txt;
        }
    }

    // ── Radar drawing (Painter2D) ─────────────────────────

    void DrawRadar(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        Rect rect   = mgc.visualElement.contentRect;
        if (rect.width < 10f) return;

        Vector2 center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
        float   radius = Mathf.Min(rect.width, rect.height) * 0.47f;

        // Background fill
        painter.BeginPath();
        painter.Arc(center, radius, 0f, 360f);
        painter.fillColor = new Color(0.04f, 0.08f, 0.06f, 0.96f);
        painter.Fill();

        // Range rings
        painter.lineWidth = 0.6f;
        for (int i = 1; i <= 3; i++)
        {
            painter.strokeColor = new Color(0.38f, 0.48f, 0.58f, 0.18f);
            painter.BeginPath();
            painter.Arc(center, radius * i / 3f, 0f, 360f);
            painter.Stroke();
        }

        // Crosshair
        painter.lineWidth   = 0.5f;
        painter.strokeColor = new Color(0.38f, 0.48f, 0.58f, 0.14f);
        painter.BeginPath();
        painter.MoveTo(new Vector2(center.x - radius, center.y));
        painter.LineTo(new Vector2(center.x + radius, center.y));
        painter.MoveTo(new Vector2(center.x, center.y - radius));
        painter.LineTo(new Vector2(center.x, center.y + radius));
        painter.Stroke();

        // Sweep sector
        if (_sweepAngle > 0f)
        {
            float trailDeg = 55f;
            float startDeg = _sweepAngle - trailDeg - 90f;
            float endDeg   = _sweepAngle - 90f;

            painter.BeginPath();
            painter.MoveTo(center);
            painter.Arc(center, radius, startDeg, endDeg);
            painter.ClosePath();
            painter.fillColor = new Color(0.22f, 0.52f, 0.42f, 0.2f);
            painter.Fill();

            // Leading edge line
            float edgeRad = (_sweepAngle - 90f) * Mathf.Deg2Rad;
            var   tip      = new Vector2(
                center.x + Mathf.Cos(edgeRad) * radius,
                center.y + Mathf.Sin(edgeRad) * radius
            );
            painter.BeginPath();
            painter.MoveTo(center);
            painter.LineTo(tip);
            painter.strokeColor = new Color(0.32f, 0.72f, 0.58f, 0.78f);
            painter.lineWidth   = 1.5f;
            painter.Stroke();
        }

        // Enemy blip (fades over time)
        if (_hasEnemyBlip)
        {
            float alpha      = Mathf.Clamp01(_enemyBlipTimer / BLIP_FADE);
            Vector2 blipPos  = new Vector2(
                center.x + _enemyBlipLocal.x * radius,
                center.y - _enemyBlipLocal.y * radius
            );

            // Outer halo
            painter.BeginPath();
            painter.Arc(blipPos, 7f, 0f, 360f);
            painter.fillColor = new Color(0.82f, 0.22f, 0.18f, 0.16f * alpha);
            painter.Fill();

            // Core dot
            painter.BeginPath();
            painter.Arc(blipPos, 3.5f, 0f, 360f);
            painter.fillColor = new Color(0.85f, 0.25f, 0.22f, alpha);
            painter.Fill();
        }

        // Wall blip (forward = top)
        if (_hasWallBlip)
        {
            var wallPos = new Vector2(center.x, center.y - radius * 0.78f);
            painter.BeginPath();
            painter.Arc(wallPos, 3f, 0f, 360f);
            painter.fillColor = new Color(0.52f, 0.70f, 0.84f, 0.82f);
            painter.Fill();
        }

        // Center dot — player position
        painter.BeginPath();
        painter.Arc(center, 3.2f, 0f, 360f);
        painter.fillColor = new Color(0.62f, 0.80f, 0.90f, 0.92f);
        painter.Fill();

        // Outer border ring
        painter.BeginPath();
        painter.Arc(center, radius, 0f, 360f);
        painter.strokeColor = new Color(0.38f, 0.52f, 0.62f, 0.32f);
        painter.lineWidth   = 1.5f;
        painter.Stroke();
    }
}
