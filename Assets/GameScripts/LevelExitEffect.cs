using UnityEngine;

// =========================================================
//  LevelExitEffect — Exit gate visual upgrade
//  Same exit gate par LevelExit.cs ke saath lagao.
//
//  Automatically adds:
//   - Pulsing point light (green=open, red=locked)
//   - Rising particle columns (underwater energy streams)
//   - Proximity HUD notifications
//   - Level 2 lock/unlock visual state + burst effect
//   - Slow rotating glow ring around gate base
//
//  SETUP: Simply attach this script to the same Exit Gate
//  GameObject that already has LevelExit.cs. No other setup needed.
// =========================================================
[RequireComponent(typeof(LevelExit))]
public class LevelExitEffect : MonoBehaviour
{
    [Header("Colors")]
    public Color unlockedColor = new Color(0.15f, 1f, 0.65f);   // teal-green
    public Color lockedColor   = new Color(1f, 0.18f, 0.08f);   // red

    [Header("Light")]
    public float lightRange      = 12f;
    public float pulseSpeed      = 1.6f;

    [Header("Particles")]
    public int   particleCount   = 35;
    public float columnRadius    = 1.8f;

    [Header("Proximity")]
    public float proximityRange  = 9f;

    // ── Internal ────────────────────────────────────────────────────
    LevelExit         _exit;
    Light             _mainLight;
    Light             _fillLight;
    ParticleSystem    _columns;
    ParticleSystem    _ring;
    GameObject        _rotator;
    Transform         _player;
    bool              _isLocked;
    float             _proxCooldown;
    static readonly int _maxParticles = 80;

    // ────────────────────────────────────────────────────────────────
    void Awake()
    {
        _exit = GetComponent<LevelExit>();
    }

    void Start()
    {
        _isLocked = _exit.requireTanks;

        var p = GameObject.FindWithTag("Player");
        if (p != null) _player = p.transform;

        BuildMainLight();
        BuildFillLight();
        BuildColumnParticles();
        BuildRingParticles();
        BuildRotatingRing();

        ApplyColorInstant();
    }

    void Update()
    {
        // ── Lock/Unlock state check (Level 2) ──────────────────────
        bool locked = _exit.requireTanks && TankCollector.Collected < _exit.requiredTanks;
        if (locked != _isLocked)
        {
            _isLocked = locked;
            ApplyColorInstant();
            if (!locked) OnUnlocked();
        }

        // ── Pulse main light ───────────────────────────────────────
        if (_mainLight != null)
        {
            float t  = 0.65f + Mathf.Sin(Time.time * pulseSpeed) * 0.35f;
            _mainLight.intensity = (_isLocked ? 1.4f : 2.8f) * t;
        }

        // ── Fill light gentle throb ────────────────────────────────
        if (_fillLight != null)
        {
            float t2 = 0.5f + Mathf.Sin(Time.time * pulseSpeed * 0.7f + 1.2f) * 0.5f;
            _fillLight.intensity = 0.6f * t2;
        }

        // ── Rotate glow ring ───────────────────────────────────────
        if (_rotator != null)
            _rotator.transform.Rotate(Vector3.up, 28f * Time.deltaTime);

        // ── Proximity notification ─────────────────────────────────
        if (_proxCooldown > 0f) { _proxCooldown -= Time.deltaTime; return; }
        if (_player == null) { var p = GameObject.FindWithTag("Player"); if (p != null) _player = p.transform; return; }

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist < proximityRange)
        {
            if (_isLocked)
                NotificationUI.Show(
                    "EXIT LOCKED — Collect all tanks first! (" +
                    TankCollector.Collected + "/" + _exit.requiredTanks + ")",
                    NotificationUI.NotifType.Danger);
            else
                NotificationUI.Show("EXIT NEARBY — Walk through to complete the level!",
                    NotificationUI.NotifType.Warning);
            _proxCooldown = 8f;
        }
    }

    // ── Light builders ───────────────────────────────────────────────

    void BuildMainLight()
    {
        var go = new GameObject("ExitMainLight");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0f, 2f, 0f);

        _mainLight             = go.AddComponent<Light>();
        _mainLight.type        = LightType.Point;
        _mainLight.range       = lightRange;
        _mainLight.shadows     = LightShadows.None;
        _mainLight.intensity   = 2.8f;
    }

    void BuildFillLight()
    {
        var go = new GameObject("ExitFillLight");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0f, 0.3f, 0f);

        _fillLight           = go.AddComponent<Light>();
        _fillLight.type      = LightType.Point;
        _fillLight.range     = lightRange * 0.55f;
        _fillLight.shadows   = LightShadows.None;
        _fillLight.intensity = 0.6f;
    }

    // ── Particle builders ────────────────────────────────────────────

    void BuildColumnParticles()
    {
        var go = new GameObject("ExitColumns");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        _columns = go.AddComponent<ParticleSystem>();

        var main             = _columns.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(1.4f, 2.2f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1.8f, 3.2f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
        main.maxParticles    = _maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.12f;

        var emission       = _columns.emission;
        emission.rateOverTime = particleCount;

        var shape          = _columns.shape;
        shape.shapeType    = ParticleSystemShapeType.Circle;
        shape.radius       = columnRadius;
        shape.radiusThickness = 0.4f;

        // Size shrinks as particle rises (tapers off)
        var sizeLife       = _columns.sizeOverLifetime;
        sizeLife.enabled   = true;
        var sizeCurve      = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.2f);
        sizeCurve.AddKey(0.4f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeLife.size      = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Alpha: fade in → hold → fade out
        var col            = _columns.colorOverLifetime;
        col.enabled        = true;
        var grad           = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] {
                new GradientAlphaKey(0f,    0f),
                new GradientAlphaKey(0.7f,  0.25f),
                new GradientAlphaKey(0.7f,  0.75f),
                new GradientAlphaKey(0f,    1f)
            });
        col.color          = grad;

        _columns.Play();
    }

    void BuildRingParticles()
    {
        var go = new GameObject("ExitRing");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0f, 0.15f, 0f);

        _ring = go.AddComponent<ParticleSystem>();

        var main             = _ring.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
        main.maxParticles    = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission         = _ring.emission;
        emission.rateOverTime = 15f;

        var shape            = _ring.shape;
        shape.shapeType      = ParticleSystemShapeType.Circle;
        shape.radius         = columnRadius * 0.9f;
        shape.radiusThickness = 0.1f;   // thin ring

        var col              = _ring.colorOverLifetime;
        col.enabled          = true;
        var grad             = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] {
                new GradientAlphaKey(0f,   0f),
                new GradientAlphaKey(0.5f, 0.3f),
                new GradientAlphaKey(0f,   1f)
            });
        col.color = grad;

        _ring.Play();
    }

    void BuildRotatingRing()
    {
        _rotator = new GameObject("ExitRotator");
        _rotator.transform.SetParent(transform);
        _rotator.transform.localPosition = new Vector3(0f, 0.1f, 0f);

        // 8 small glowing points arranged in a circle around base
        int points = 8;
        float r    = columnRadius * 1.05f;
        for (int i = 0; i < points; i++)
        {
            float angle = i * (360f / points) * Mathf.Deg2Rad;
            var pt      = new GameObject("RingPoint_" + i);
            pt.transform.SetParent(_rotator.transform);
            pt.transform.localPosition = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);

            var l       = pt.AddComponent<Light>();
            l.type      = LightType.Point;
            l.range     = 1.8f;
            l.intensity = 0.5f;
            l.shadows   = LightShadows.None;
        }
    }

    // ── Color application ────────────────────────────────────────────

    void ApplyColorInstant()
    {
        Color c = _isLocked ? lockedColor : unlockedColor;

        if (_mainLight != null) _mainLight.color = c;
        if (_fillLight != null) _fillLight.color = c;

        // Tint ring point lights
        if (_rotator != null)
            foreach (Light l in _rotator.GetComponentsInChildren<Light>())
                l.color = c;

        // Recolor column particles
        SetParticleColor(_columns, c);
        SetParticleColor(_ring,    c);
    }

    void SetParticleColor(ParticleSystem ps, Color c)
    {
        if (ps == null) return;
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(c.r, c.g, c.b, 0.25f),
            new Color(c.r, c.g, c.b, 0.85f));
    }

    // ── Unlock event (Level 2) ───────────────────────────────────────

    void OnUnlocked()
    {
        // Burst of 50 particles
        if (_columns != null) _columns.Emit(50);
        if (_ring    != null) _ring.Emit(30);

        NotificationUI.Show("EXIT UNLOCKED — Head to the exit now!", NotificationUI.NotifType.Warning);
        CameraShake.Shake(0.25f, 0.10f);

        // Temporarily boost light intensity for flash effect
        StartCoroutine(UnlockFlash());
    }

    System.Collections.IEnumerator UnlockFlash()
    {
        if (_mainLight == null) yield break;
        float original = _mainLight.intensity;
        _mainLight.intensity = 8f;
        yield return new WaitForSeconds(0.12f);
        _mainLight.intensity = 5f;
        yield return new WaitForSeconds(0.12f);
        _mainLight.intensity = original;
    }
}
